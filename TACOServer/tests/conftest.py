import io
import os
import pytest
import numpy as np
from PIL import Image
from unittest.mock import MagicMock
import onnxruntime as rt


_PROJECT_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

_TEST_FRAME   = os.path.join(_PROJECT_ROOT, "tests\\test_frame.jpg")
_MODEL_PATH   = os.path.join(_PROJECT_ROOT, "model.onnx")


def is_npu_available() -> bool:
    return "QNNExecutionProvider" in rt.get_available_providers()


npu_only = pytest.mark.skipif(
    not is_npu_available(),
    reason="QNNExecutionProvider not available on this machine",
)


@pytest.fixture
def sample_jpeg() -> bytes:
    """Real camera frame from test_frame.jpg."""
    with open(_TEST_FRAME, "rb") as f:
        return f.read()


@pytest.fixture
def small_jpeg() -> bytes:
    """Smaller than model input to test upscaling."""
    img = Image.new("RGB", (64, 64), color=(200, 150, 100))
    buf = io.BytesIO()
    img.save(buf, format="JPEG")
    return buf.getvalue()


@pytest.fixture
def tall_jpeg() -> bytes:
    """Portrait image to test horizontal padding."""
    img = Image.new("RGB", (200, 800), color=(50, 100, 200))
    buf = io.BytesIO()
    img.save(buf, format="JPEG")
    return buf.getvalue()


@pytest.fixture
def wide_jpeg() -> bytes:
    """Landscape image to test vertical padding."""
    img = Image.new("RGB", (1280, 200), color=(50, 200, 100))
    buf = io.BytesIO()
    img.save(buf, format="JPEG")
    return buf.getvalue()


@pytest.fixture
def fake_onnx_outputs():
    """Fake model outputs with zero-point values so scores decode to 0.0."""
    heatmaps = np.full((1, 17, 33, 17), 178, dtype=np.uint8)
    offsets  = np.full((1, 34, 33, 17), 127, dtype=np.uint8)
    return [heatmaps, offsets]


@pytest.fixture
def mock_session(fake_onnx_outputs):
    """Fake ONNX session — returns fake outputs without touching model.onnx."""
    sess = MagicMock(spec=rt.InferenceSession)
    sess.run.return_value = fake_onnx_outputs
    sess.get_providers.return_value = ["CPUExecutionProvider"]
    return sess