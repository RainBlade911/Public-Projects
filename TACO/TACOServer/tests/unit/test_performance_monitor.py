from observers.performance_monitor import PerformanceMonitor


def test_timings_are_recorded():
    monitor = PerformanceMonitor(label="CPU", log_every=100)

    timings = {
        "preprocess": 10,
        "inference": 20,
        "decode": 5,
        "total": 40,
    }

    monitor.on_inference_complete(timings, "CPU")

    assert len(monitor.times["preprocess"]) == 1
    assert monitor.times["preprocess"][0] == 10


def test_frame_count_increments():
    monitor = PerformanceMonitor(label="CPU", log_every=100)

    monitor.on_inference_complete({}, "CPU")
    monitor.on_inference_complete({}, "CPU")

    assert monitor.frame_count == 2


def test_rolling_window_limits_size():
    monitor = PerformanceMonitor(label="CPU", log_every=100, window=3)

    for i in range(5):
        monitor.on_inference_complete({"preprocess": i}, "CPU")

    # should only keep last 3 values
    assert len(monitor.times["preprocess"]) == 3

from unittest.mock import patch


def test_print_summary_called_at_interval():
    monitor = PerformanceMonitor(label="CPU", log_every=2)

    with patch.object(monitor, "_print_summary") as mock_print:
        monitor.on_inference_complete({}, "CPU")  # frame 1
        monitor.on_inference_complete({}, "CPU")  # frame 2 → should trigger

        mock_print.assert_called_once()

def test_unknown_keys_are_ignored():
    monitor = PerformanceMonitor(label="CPU", log_every=100)

    monitor.on_inference_complete({"random": 123}, "CPU")

    # should not crash and not store anything
    for stage in monitor.STAGES:
        assert len(monitor.times[stage]) == 0

# tests/unit/test_performance_monitor_summary.py

from observers.performance_monitor import PerformanceMonitor

def test_print_summary_executes_and_uses_data(capsys):
    monitor = PerformanceMonitor(label="CPU", log_every=1)

    # Feed multiple frames so averages are meaningful
    monitor.on_inference_complete({
        "preprocess": 10,
        "inference": 20,
        "decode": 5,
        "total": 40,
    }, "CPU")

    monitor.on_inference_complete({
        "preprocess": 20,
        "inference": 30,
        "decode": 10,
        "total": 60,
    }, "CPU")

    # Force summary (since log_every=1, it already ran twice)
    # Capture stdout so we don't clutter test output
    captured = capsys.readouterr()

    # Basic sanity: something was printed
    assert captured.out.strip() != ""