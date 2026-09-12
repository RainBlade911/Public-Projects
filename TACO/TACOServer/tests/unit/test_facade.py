from unittest.mock import MagicMock
from facade import PoseEstimationFacade


def test_estimate_returns_expected_structure():
    mock_pipeline = MagicMock()

    mock_ctx = MagicMock()
    mock_ctx.keypoints = [{"id": 0}]
    mock_ctx.timings = {
        "preprocess": 10,
        "inference": 20,
    }

    mock_pipeline.run.return_value = mock_ctx

    facade = PoseEstimationFacade(sess=None, runner=None, backend_label="CPU")
    facade._pipeline = mock_pipeline  # inject mock

    result = facade.estimate(b"image")

    assert "latency_ms" in result
    assert "keypoints" in result
    assert "backend" in result
    assert "perf" in result

def test_pipeline_is_called():
    mock_pipeline = MagicMock()
    mock_pipeline.run.return_value = MagicMock(keypoints=[], timings={})

    facade = PoseEstimationFacade(None, None, "CPU")
    facade._pipeline = mock_pipeline

    facade.estimate(b"data")

    mock_pipeline.run.assert_called_once()

def test_keypoints_propagated():
    mock_pipeline = MagicMock()

    mock_ctx = MagicMock()
    mock_ctx.keypoints = [{"id": 5}]
    mock_ctx.timings = {}

    mock_pipeline.run.return_value = mock_ctx

    facade = PoseEstimationFacade(None, None, "CPU")
    facade._pipeline = mock_pipeline

    result = facade.estimate(b"data")

    assert result["keypoints"][0]["id"] == 5


def test_observer_is_notified():
    mock_pipeline = MagicMock()

    mock_ctx = MagicMock()
    mock_ctx.keypoints = []
    mock_ctx.timings = {"total": 100}

    mock_pipeline.run.return_value = mock_ctx

    mock_observer = MagicMock()

    facade = PoseEstimationFacade(None, None, "CPU")
    facade._pipeline = mock_pipeline
    facade.subscribe(mock_observer)

    facade.estimate(b"data")

    mock_observer.on_inference_complete.assert_called_once()

def test_backend_label_passed():
    mock_pipeline = MagicMock()
    mock_pipeline.run.return_value = MagicMock(keypoints=[], timings={})

    facade = PoseEstimationFacade(None, None, "NPU")
    facade._pipeline = mock_pipeline

    result = facade.estimate(b"data")

    assert result["backend"] == "NPU"

def test_perf_values_present():
    mock_pipeline = MagicMock()

    mock_ctx = MagicMock()
    mock_ctx.keypoints = []
    mock_ctx.timings = {
        "preprocess": 12.3,
        "inference": 45.6,
    }

    mock_pipeline.run.return_value = mock_ctx

    facade = PoseEstimationFacade(None, None, "CPU")
    facade._pipeline = mock_pipeline

    result = facade.estimate(b"data")

    assert "pre_ms" in result["perf"]
    assert "inf_ms" in result["perf"]
    assert "total_ms" in result["perf"]