from core.pipeline_context import PipelineContext
from stages.preprocess_stage import PreprocessStage
from stages.inference_stage import InferenceStage
from stages.decode_stage import DecodeStage
from inference.inference_strategy import CPUInferenceRunner


class TestDecodeStage:

    def _run(self, sample_jpeg, mock_session):
        ctx = PipelineContext(jpeg_bytes=sample_jpeg)
        ctx = PreprocessStage().process(ctx)
        ctx = InferenceStage(mock_session, CPUInferenceRunner()).process(ctx)
        ctx = DecodeStage().process(ctx)
        return ctx

    def test_returns_17_keypoints(self, sample_jpeg, mock_session):
        result = self._run(sample_jpeg, mock_session)
        assert len(result.keypoints) == 17

    def test_each_keypoint_has_id(self, sample_jpeg, mock_session):
        result = self._run(sample_jpeg, mock_session)
        for kp in result.keypoints:
            assert "id" in kp

    def test_each_keypoint_has_x_and_y(self, sample_jpeg, mock_session):
        result = self._run(sample_jpeg, mock_session)
        for kp in result.keypoints:
            assert "x" in kp
            assert "y" in kp

    def test_each_keypoint_has_score(self, sample_jpeg, mock_session):
        result = self._run(sample_jpeg, mock_session)
        for kp in result.keypoints:
            assert "score" in kp

    def test_keypoint_ids_are_0_to_16(self, sample_jpeg, mock_session):
        result = self._run(sample_jpeg, mock_session)
        ids = [kp["id"] for kp in result.keypoints]
        assert ids == list(range(17))

    def test_decode_timing_is_recorded(self, sample_jpeg, mock_session):
        result = self._run(sample_jpeg, mock_session)
        assert "decode" in result.timings
        assert result.timings["decode"] >= 0