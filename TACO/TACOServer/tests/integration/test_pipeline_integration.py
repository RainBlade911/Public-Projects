import numpy as np
from core.pipeline import Pipeline
from core.pipeline_context import PipelineContext
from stages.preprocess_stage import PreprocessStage
from stages.inference_stage import InferenceStage
from stages.decode_stage import DecodeStage
from inference.inference_strategy import CPUInferenceRunner


def test_full_pipeline_runs(sample_jpeg, mock_session):
    pipeline = Pipeline([
        PreprocessStage(),
        InferenceStage(mock_session, CPUInferenceRunner()),
        DecodeStage(),
    ])

    ctx = PipelineContext(jpeg_bytes=sample_jpeg)
    result = pipeline.run(ctx)

    assert result.tensor is not None
    assert result.raw_outputs is not None
    assert result.keypoints is not None
    assert len(result.keypoints) == 17

def test_data_flows_between_stages(sample_jpeg, mock_session):
    pipeline = Pipeline([
        PreprocessStage(),
        InferenceStage(mock_session, CPUInferenceRunner()),
        DecodeStage(),
    ])

    ctx = PipelineContext(jpeg_bytes=sample_jpeg)
    result = pipeline.run(ctx)

    # ensure preprocess output used in inference
    assert result.tensor.shape[0] == 1

    # ensure inference output used in decode
    assert isinstance(result.keypoints, list)

def test_pipeline_timings_present(sample_jpeg, mock_session):
    pipeline = Pipeline([
        PreprocessStage(),
        InferenceStage(mock_session, CPUInferenceRunner()),
        DecodeStage(),
    ])

    ctx = PipelineContext(jpeg_bytes=sample_jpeg)
    result = pipeline.run(ctx)

    assert "preprocess" in result.timings
    assert "inference" in result.timings
    assert "decode" in result.timings

def test_keypoints_are_normalized(sample_jpeg, mock_session):
    pipeline = Pipeline([
        PreprocessStage(),
        InferenceStage(mock_session, CPUInferenceRunner()),
        DecodeStage(),
    ])

    ctx = PipelineContext(jpeg_bytes=sample_jpeg)
    result = pipeline.run(ctx)

    for kp in result.keypoints:
        assert 0.0 <= kp["x"] <= 1.0
        assert 0.0 <= kp["y"] <= 1.0