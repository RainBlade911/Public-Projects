from core.stage import Stage
from core.pipeline_context import PipelineContext
from inference.inference_strategy import InferenceRunner


class InferenceStage(Stage):
    """
    Pipeline Stage — runs the ONNX session via whichever InferenceRunner
    (Strategy) was injected at construction time.
    Writes raw_outputs and inf_ms into the context.
    """

    def __init__(self, sess, runner: InferenceRunner):
        self._sess   = sess
        self._runner = runner

    def process(self, context: PipelineContext) -> PipelineContext:
        outputs, inf_ms = self._runner.run(context.tensor, self._sess)
        context.raw_outputs = outputs
        context.inf_ms      = inf_ms
        context.timings["inference"] = inf_ms
        return context