# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import pytest
from azuresphere_device_api import utils
from azuresphere_device_api.exceptions import ValidationError
from packaging import version
from sys import platform


def test__sdk_version_resolves():
    """Tests if retrieving the SDK version succeeds."""
    version = utils.get_sdk_version()
    assert version is not None


def test__sdk_version_greater_than_2211():
    """Compares the SDK version against an expected value."""
    sdk_version = utils.get_sdk_version()
    assert sdk_version >= version.parse("22.11.0")


def test__sdk_version_multiple_calls_same():
    """Tests if retrieving the SDK version multiple times returns the same value."""
    assert utils.get_sdk_version() == utils.get_sdk_version()


def test__get_device_throws_on_old():
    """Tests if setting ip on old SDK throws exception on Linux."""
    if "linux" in platform:
        utils._SDK_VERSION = version.parse("22.11.0")
        with pytest.raises(ValidationError):
            utils.set_device_ip_address("192.168.35.5")
        utils._SDK_VERSION = None
