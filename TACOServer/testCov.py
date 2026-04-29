import pytest

if __name__ == "__main__":
    raise SystemExit(pytest.main([
        "--cov=.",
        "--cov-report=term-missing",
        "--cov-config=.coveragerc"
    ]))