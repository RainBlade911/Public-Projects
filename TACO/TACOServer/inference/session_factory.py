from abc import ABC, abstractmethod
import onnxruntime as rt

MODEL_PATH = "model.onnx"


# ─────────────────────────────────────────────
# Factory Method Pattern
# ─────────────────────────────────────────────

class SessionFactory(ABC):
    """
    Factory Method Pattern — declares the interface for creating an
    ONNX InferenceSession. Subclasses decide which providers and
    options to use; callers never see construction details.
    """

    def create(self) -> rt.InferenceSession:
        options = self._make_options()
        providers, provider_options = self._make_providers()
        sess = rt.InferenceSession(
            MODEL_PATH,
            sess_options=options,
            providers=providers,
            provider_options=provider_options,
        )
        self._on_created(sess)
        return sess

    @abstractmethod
    def _make_options(self) -> rt.SessionOptions: ...

    @abstractmethod
    def _make_providers(self) -> tuple[list, list]: ...

    def _on_created(self, sess: rt.InferenceSession):
        print("Session ready. Providers:", sess.get_providers())


class NPUSessionFactory(SessionFactory):
    """Factory for Qualcomm QNN / HTP (NPU) sessions."""

    def _make_options(self) -> rt.SessionOptions:
        options = rt.SessionOptions()
        options.graph_optimization_level = rt.GraphOptimizationLevel.ORT_ENABLE_ALL
        return options

    def _make_providers(self) -> tuple[list, list]:
        provider_options = {
            "backend_path":       "QnnHtp.dll",
            "htp_performance_mode": "burst",
            "profiling_level":    "off",
            "htp_arch":           "v73",
        }
        return (
            ["QNNExecutionProvider", "CPUExecutionProvider"],
            [provider_options, {}],
        )

    def _on_created(self, sess):
        print("NPU session ready. Providers:", sess.get_providers())


class CPUSessionFactory(SessionFactory):
    """Factory for plain CPU sessions."""

    def _make_options(self) -> rt.SessionOptions:
        options = rt.SessionOptions()
        options.graph_optimization_level = rt.GraphOptimizationLevel.ORT_DISABLE_ALL
        return options

    def _make_providers(self) -> tuple[list, list]:
        return ["CPUExecutionProvider"], [{}]

    def _on_created(self, sess):
        print("CPU session ready. Providers:", sess.get_providers())