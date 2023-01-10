# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import pytest
from azuresphere_device_api import exceptions, manufacturing


def test__set_manufacturing_state__null_state_throws_validation_error():
    """Tests if setting the manufacturing state with 'None' throws a validation error."""
    with pytest.raises(exceptions.ValidationError, match="ERROR: Cannot set manufacturing state, manufacturing state supplied is invalid"):
        manufacturing.set_device_manufacturing_state(None)


def test__set_manufacturing_state__empty_state_throws_validation_error():
    """Tests if setting the manufacturing state with an empty string throws a valiadation error."""
    with pytest.raises(exceptions.ValidationError, match="ERROR: Cannot set manufacturing state, manufacturing state supplied is invalid"):
        manufacturing.set_device_manufacturing_state("")


def test__set_manufacturing_state__invalid_state_throws_validation_error():
    """Tests if setting the manufacturing state with an invalid value throws a validation error."""
    with pytest.raises(exceptions.ValidationError, match="ERROR: Cannot set manufacturing state, manufacturing state supplied is invalid"):
        manufacturing.set_device_manufacturing_state("InvalidState!")
