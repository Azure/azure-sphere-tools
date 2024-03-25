# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from enum import Enum, auto, unique
from http import HTTPStatus
from pathlib import Path
from typing import Any
from uuid import UUID
from sys import platform
import requests
from requests_toolbelt.adapters import host_header_ssl

from azuresphere_device_api.error_handler import handle_status_code_errors
from azuresphere_device_api.exceptions import UnknownDeviceError, DeviceError, AzureSphereDeviceApiException, ValidationError
from azuresphere_device_api.validation import set_current_device_api_version

from packaging import version
import os

_CURRENT_DEVICE_IP = "192.168.35.2"
_SDK_VERSION = None

CURRENT_DIR = Path(__file__).parent
_CERT_PATH = (CURRENT_DIR / "certs/device_rest_api_certificate.pem").absolute()

_REQUEST_TIMEOUT = 15.


@unique
class AzureSphereDeviceApiRequestType(Enum):

    """Azure Sphere Device Api Request Type."""
    LOCAL_DEVICE_URL = auto()
    DEVICE_URL = auto()
    DEVICE_REST_API_VERSION = auto()


__all__ = ['get_response_code_content', 'set_device_ip_address', 'get_device_ip_address', 'get_request', 'delete_request',
           'post_request_no_body', 'post_request', 'patch_request', 'put_request', 'put_request_octet_stream', 'is_uuid', 'AzureSphereDeviceApiRequestType']

__active_session = None


def __device_rest_api_headers() -> dict:
    return {
        "Host": "*.devices.sphere.azure.local", 'Accept': 'application/json'}


def __get_session() -> requests.Session:
    global __active_session
    if __active_session is None:
        __active_session = requests.Session()
        __active_session.mount(
            "https://", host_header_ssl.HostHeaderSSLAdapter())
    return __active_session


def __make_request(**kwargs):
    try:
        return __get_session().request(**kwargs)
    except requests.exceptions.ConnectionError as conn_err:
        raise DeviceError(
            f"Device connection timed out for {get_device_ip_address()}. Please ensure your device is connected and has development mode enabled.") from conn_err
    except Exception as other_exception:
        raise UnknownDeviceError from other_exception


def get_sdk_path() -> str:
    sdk_path = os.environ.get("AzureSphereDefaultSDKDir")

    if sdk_path == "" or sdk_path is None:
        raise AzureSphereDeviceApiException(
            "Cannot retrieve the SDK path, 'AzureSphereDefaultSDKDir' environment variable is not set! Is the SDK installed and available?")

    return sdk_path


def get_sdk_version() -> version.Version:
    global _SDK_VERSION
    if _SDK_VERSION is not None:
        return _SDK_VERSION

    sdk_version_path = os.path.join(get_sdk_path(), "VERSION")

    if os.path.exists(sdk_version_path):
        with open(sdk_version_path, "r", encoding="utf8") as version_file:
            version_number = version_file.read().strip()
            period_count = len(
                [char for char in version_number if char == "."])
            if period_count > 3:
                chunked = version_number.split(".")
                preview_version = chunked[-1]
                version_number = ".".join(chunked[:3]) + f"- {preview_version}"

            _SDK_VERSION = version.parse(version_number)
            return _SDK_VERSION

    raise AzureSphereDeviceApiException(
        "Cannot retrieve the SDK version, version file does not exist! Is the SDK installed and available?")


def set_device_ip_address(ip_address: str):
    """Sets the device IP address used in all requests.

    :param ip_address: The device IP address to use.
    :type ip_address: string
    """
    global _CURRENT_DEVICE_IP

    if "linux" in platform and ip_address != "192.168.35.2" and get_sdk_version() < version.parse("23.04"):
        raise ValidationError(
            f"ERROR: Cannot set active device IP address to '{ip_address}'. This SDK version does not support multiple devices.")

    _CURRENT_DEVICE_IP = ip_address


def get_device_ip_address() -> str:
    """Returns the device IP address currently used in all requests.
    """
    return _CURRENT_DEVICE_IP


def get_response_code_content(response: requests.models.Response, api_type=AzureSphereDeviceApiRequestType.DEVICE_URL) -> Any:
    """Gets the response code and content (json-encoded or in bytes).

    :param response: The response returned from a REST request.
    :type response: requests.models.Response
    :returns: A list or dict containing the response.
    :rtype: Any
    :raises: AzureSphereDeviceApiException
    """
    if response.status_code not in [HTTPStatus.OK, HTTPStatus.CREATED]:
        handle_status_code_errors(response)

    if "REST-API-Version" in response.headers:
        set_current_device_api_version(response.headers["REST-API-Version"])

    if api_type == AzureSphereDeviceApiRequestType.DEVICE_REST_API_VERSION:
        return {} if len(response.content) == 0 else response.headers

    if 'Content-Type' in response.headers.keys() and response.headers['Content-Type'] == 'application/octet-stream':
        return {} if len(response.content) == 0 else response.content

    return {} if len(response.content) == 0 else response.json()


def _create_url(api: str = "") -> str:
    """Generates a URL using the current device IP address and the provided API.

    :param api: The api to append to the URL.
    :type api: string
    :returns: The URL string for a future request.
    :rtype: str
    """
    return f"https://{get_device_ip_address()}/{api}"


def get_request(api: str, api_type=AzureSphereDeviceApiRequestType.DEVICE_URL) -> Any:
    """Makes a "GET" request with the given API endpoint.

    :param api: The Azure Sphere Device API enum to use.
    :type api: AzureSphereDeviceApis
    :returns: A status code reflecting the success of the request and an empty string if the request
    content is null, else the content.
    :rtype: Tuple(int, dict[str, str])
    :raises: requests.exceptions
    :raises: AzureSphereDeviceApiException
    """
    # Create URL
    if api_type == AzureSphereDeviceApiRequestType.LOCAL_DEVICE_URL:
        url_to_use = f"http://localhost:48938/{api}"
        header = {'Accept': 'application/json'}
        return get_response_code_content(requests.get(
            url_to_use,
            verify=str(_CERT_PATH),
            allow_redirects=False,
            headers=header), api_type)

    return get_response_code_content(__make_request(
        method="GET",
        url=_create_url(api),
        verify=str(_CERT_PATH),
        allow_redirects=False,
        headers=__device_rest_api_headers(),
        timeout=_REQUEST_TIMEOUT
    ), api_type)


def delete_request(api: str) -> dict:
    """Makes a "DELETE" request with the given API endpoint.

    :param api: The Azure Sphere Device API enum to use.
    :type api: AzureSphereDeviceApis
    :returns: A status code reflecting the success of the request and an empty string if the request
    content is null, else the content.
    :rtype: Tuple(int, dict[str, str])
    :raises: requests.exceptions
    :raises: AzureSphereDeviceApiException
    """
    return get_response_code_content(__make_request(
        method="DELETE",
        url=_create_url(api),
        verify=str(_CERT_PATH),
        allow_redirects=False,
        headers=__device_rest_api_headers(),
        timeout=_REQUEST_TIMEOUT
    ))


def post_request_no_body(api: str) -> dict:
    """Makes a "POST" request with the given API endpoint and no JSON body.

    :param api: The Azure Sphere Device API enum to use.
    :type api: AzureSphereDeviceApis
    :returns: A status code reflecting the success of the request and an empty string if the request
    content is null, else the content.
    :rtype: Tuple(int, dict[str, str])
    :raises: requests.exceptions
    :raises: AzureSphereDeviceApiException
    """
    return get_response_code_content(__make_request(
        method="POST",
        url=_create_url(api),
        verify=str(_CERT_PATH),
        allow_redirects=False,
        headers=__device_rest_api_headers(),
        timeout=_REQUEST_TIMEOUT
    ))


def post_request(api: str, body: Any) -> dict:
    """Makes a "POST" request with the given API endpoint and json body.

    :param api: The Azure Sphere Device API enum to use.
    :type api: AzureSphereDeviceApis
    :param body: The json body to be added to the request.
    :type body: List<Dict>
    :returns: A status code reflecting the success of the request and an empty string if the request
    content is null, else the content.
    :rtype: Tuple(int, dict[str, str])
    :raises: requests.exceptions
    :raises: AzureSphereDeviceApiException
    """
    return get_response_code_content(__make_request(
        method="POST",
        url=_create_url(api),
        verify=str(_CERT_PATH),
        allow_redirects=False,
        headers=__device_rest_api_headers(),
        json=body,
        timeout=_REQUEST_TIMEOUT
    ))


def patch_request(api: str, body: Any) -> dict:
    """Makes a "PATCH" request with the given API endpoint and json body, returning the response as
    a RestResponse.

    :param api: The Azure Sphere Device API enum to use.
    :type api: AzureSphereDeviceApis
    :param body: The json body to be added to the request.
    :type body: dict[str, str]
    :returns: A status code reflecting the success of the request and an empty string if the request
    content is null, else the content.
    :rtype: Tuple(int, dict[str, str])
    :raises: requests.exceptions
    :raises: AzureSphereDeviceApiException
    """
    return get_response_code_content(__make_request(
        method="PATCH",
        url=_create_url(api),
        verify=str(_CERT_PATH),
        allow_redirects=False,
        headers=__device_rest_api_headers(),
        json=body,
        timeout=_REQUEST_TIMEOUT
    ))


def put_request(api: str, body: Any) -> dict:
    """Makes a "PUT" request with the given API endpoint and json body, returning the response as
    a RestResponse.

    :param api: The Azure Sphere Device API enum to use.
    :type api: AzureSphereDeviceApis
    :param body: The json body to be added to the request.
    :type body: dict[str, str]

    :returns: A status code reflecting the success of the request and an empty string if the request
    content is null, else the content.
    :rtype: Tuple(int, dict[str, str])
    :raises: requests.exceptions
    :raises: AzureSphereDeviceApiException
    """
    return get_response_code_content(__make_request(
        method="PUT",
        url=_create_url(api),
        verify=str(_CERT_PATH),
        allow_redirects=False,
        headers=__device_rest_api_headers(),
        json=body,
        timeout=_REQUEST_TIMEOUT
    ))


def put_request_octet_stream(api: str, body: Any) -> dict:
    """Makes a "PUT" request with the given API endpoint and json body, returning the response as
    a RestResponse.

    :param api: The Azure Sphere Device API enum to use.
    :type api: AzureSphereDeviceApis
    :param body: The json body to be added to the request.
    :type body: dict[str, str]

    :returns: A status code reflecting the success of the request and an empty string if the request
    content is null, else the content.
    :rtype: Tuple(int, dict[str, str])
    :raises: requests.exceptions
    :raises: AzureSphereDeviceApiException
    """
    devicerestapi_headers = __device_rest_api_headers()
    devicerestapi_headers["Content-Type"] = "application/octet-stream"

    return get_response_code_content(__make_request(
        method="PUT",
        url=_create_url(api),
        verify=str(_CERT_PATH),
        allow_redirects=False,
        headers=devicerestapi_headers,
        data=body,
        timeout=_REQUEST_TIMEOUT
    ))


def is_uuid(uuid: str) -> bool:
    """Validates if a given string represents a valid UUID.

    :param uuid: String being validated against being a valid UUID format.
    :type uuid: string
    :returns: True if string is valid UUID, or False otherwise
    :rtype: bool
    """
    try:
        _ = UUID(uuid)
        return True
    except (ValueError, TypeError):
        return False


def set_request_timeout(timeout: float) -> None:
    """Sets the timeout for requests.

    :param timeout: The timeout in seconds.
    :type timeout: float
    """
    global _REQUEST_TIMEOUT
    _REQUEST_TIMEOUT = timeout


def get_request_timeout() -> float:
    """Gets the timeout for requests.

    :returns: The timeout in seconds.
    :rtype: float
    """
    return _REQUEST_TIMEOUT
