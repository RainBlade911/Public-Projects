import pytest
from unittest.mock import patch, MagicMock
from inference.session_factory import CPUSessionFactory


@patch("inference.session_factory.rt.InferenceSession")
def test_cpu_factory_creates_session(mock_session_class):
    mock_session = MagicMock()
    mock_session.get_providers.return_value = ["CPUExecutionProvider"]
    mock_session_class.return_value = mock_session

    factory = CPUSessionFactory()
    sess = factory.create()

    # session returned
    assert sess == mock_session

    # InferenceSession called
    mock_session_class.assert_called_once()

    # check providers passed in
    _, kwargs = mock_session_class.call_args
    assert kwargs["providers"] == ["CPUExecutionProvider"]

from inference.session_factory import NPUSessionFactory


@patch("inference.session_factory.rt.InferenceSession")
def test_npu_factory_creates_session(mock_session_class):
    mock_session = MagicMock()
    mock_session.get_providers.return_value = ["QNNExecutionProvider", "CPUExecutionProvider"]
    mock_session_class.return_value = mock_session

    factory = NPUSessionFactory()
    sess = factory.create()

    assert sess == mock_session

    _, kwargs = mock_session_class.call_args

    # providers order matters
    assert "QNNExecutionProvider" in kwargs["providers"]
    assert "CPUExecutionProvider" in kwargs["providers"]


@patch("inference.session_factory.rt.InferenceSession")
def test_session_options_are_passed(mock_session_class):
    mock_session_class.return_value = MagicMock()

    factory = CPUSessionFactory()
    factory.create()

    _, kwargs = mock_session_class.call_args

    assert "sess_options" in kwargs
    assert kwargs["sess_options"] is not None

@patch("inference.session_factory.rt.InferenceSession")
def test_npu_provider_options_exist(mock_session_class):
    mock_session_class.return_value = MagicMock()

    factory = NPUSessionFactory()
    factory.create()

    _, kwargs = mock_session_class.call_args

    provider_options = kwargs["provider_options"]

    assert isinstance(provider_options, list)
    assert len(provider_options) == 2  # QNN + CPU fallback
    assert "backend_path" in provider_options[0]




from unittest.mock import MagicMock, patch
from inference.session_factory import CPUSessionFactory

@patch("inference.session_factory.rt.InferenceSession")
def test_on_created_prints_providers(mock_session_class, capsys):
    mock_session = MagicMock()
    mock_session.get_providers.return_value = ["CPUExecutionProvider"]
    mock_session_class.return_value = mock_session

    factory = CPUSessionFactory()
    factory.create()

    captured = capsys.readouterr()

    assert "CPU session ready" in captured.out
    assert "CPUExecutionProvider" in captured.out