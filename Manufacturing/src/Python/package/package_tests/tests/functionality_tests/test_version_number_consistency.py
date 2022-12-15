from azuresphere_device_api import version
import toml

def test__versions_are_consistent__():
    """Tests versions are consistent between pyproject.toml and the package stored version"""
    toml_file = toml.load("pyproject.toml")
    assert toml_file["project"]["version"] == version