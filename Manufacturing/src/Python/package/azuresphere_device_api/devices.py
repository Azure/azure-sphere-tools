# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import re
from sys import platform

from azuresphere_device_api import utils
from azuresphere_device_api.exceptions import ValidationError


def set_active_device_ip_address(ip_address: str) -> None:
    """Sets the device IP address used in all requests.

    :param ip_address: The device IP address to use.
    :type ip_address: string
    """

    regex_pattern = '^192.168.35.\\b([2-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\\b$'
    pattern_matches = re.findall(regex_pattern, ip_address)

    if not pattern_matches:
        raise ValidationError(
            f"ERROR: Cannot set active device IP address {ip_address}, range is 192.168.35.2-192.168.35.255")

    utils.set_device_ip_address(ip_address)


def get_active_device_ip_address() -> str:
    """Returns the device IP address currently used in all requests.

    :returns: A string containing the current IP address.
    :rtype: str
    """
    return utils.get_device_ip_address()


def get_attached_devices() -> list:
    """Makes a "GET" request to list the attached devices.

    :return: List of attached devices on success. An exception will be thrown on error.
    message.
    :rtype: Tuple(int, dict[str, str])
    :raises: requests.exceptions
    :raises: AzureSphereDeviceApiException
    """
    if platform == "win32" or "linux" in platform:
        return utils.get_request("api/service/devices", api_type=utils.AzureSphereDeviceApiRequestType.LOCAL_DEVICE_URL)

    raise ValidationError(
        "ERROR: Cannot get attached devices, unsupported operating system.")