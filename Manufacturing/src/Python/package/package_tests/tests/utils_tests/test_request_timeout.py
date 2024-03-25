# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import pytest
from azuresphere_device_api import utils, device


def test__request_timeout_is_applied_correctly():
    """Tests setting a 0 timeout causes an exception."""
    utils.set_request_timeout(0)
    assert utils.get_request_timeout() == 0
    with pytest.raises(Exception):
        device.get_device_status()


def test__request_timeout_removed_correctly():
    """Tests setting a normal timeout succeeds."""
    utils.set_request_timeout(10)
    device.get_device_status()
