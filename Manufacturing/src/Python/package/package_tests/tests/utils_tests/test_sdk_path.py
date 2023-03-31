# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import pytest
from azuresphere_device_api import utils


def test__sdk_path_resolves_correctly():
    """Tests if retrieving the SDK path succeeds."""
    path = utils.get_sdk_path()
    assert path is not None
    assert len(path) > 3
