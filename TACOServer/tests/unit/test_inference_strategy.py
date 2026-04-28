import numpy as np
from unittest.mock import MagicMock
from inference.inference_strategy import CPUInferenceRunner


def test_run_returns_outputs_and_time():
    runner = CPUInferenceRunner()

    mock_sess = MagicMock()
    mock_sess.run.return_value = ["out1", "out2"]

    tensor = np.zeros((1, 3, 10, 10), dtype=np.uint8)

    outputs, inf_ms = runner.run(tensor, mock_sess)

    assert outputs == ["out1", "out2"]
    assert isinstance(inf_ms, float)
    assert inf_ms >= 0

def test_session_called_with_correct_input_key():
    runner = CPUInferenceRunner()

    mock_sess = MagicMock()
    mock_sess.run.return_value = ["out"]

    tensor = np.zeros((1, 3, 10, 10), dtype=np.uint8)

    runner.run(tensor, mock_sess)

    args, _ = mock_sess.run.call_args

    # args[1] is the input dict
    input_dict = args[1]

    assert "image" in input_dict
    assert input_dict["image"] is tensor

    
from inference.inference_strategy import InferenceRunner


def test_template_method_calls_internal_run():
    class TestRunner(InferenceRunner):
        def _run_session(self, tensor, sess):
            return ["called"]

    runner = TestRunner()
    mock_sess = MagicMock()

    outputs, _ = runner.run(None, mock_sess)

    assert outputs == ["called"]

from inference.inference_strategy import CPUInferenceRunner, NPUInferenceRunner


def test_cpu_and_npu_strategies_equivalent():
    cpu_runner = CPUInferenceRunner()
    npu_runner = NPUInferenceRunner()

    mock_sess = MagicMock()
    mock_sess.run.return_value = ["same"]

    tensor = np.zeros((1, 3, 10, 10), dtype=np.uint8)

    out_cpu, _ = cpu_runner.run(tensor, mock_sess)
    out_npu, _ = npu_runner.run(tensor, mock_sess)

    assert out_cpu == out_npu

def test_inference_time_is_positive():
    runner = CPUInferenceRunner()

    mock_sess = MagicMock()
    mock_sess.run.return_value = ["out"]

    tensor = np.zeros((1, 3, 10, 10), dtype=np.uint8)

    _, inf_ms = runner.run(tensor, mock_sess)

    assert inf_ms >= 0