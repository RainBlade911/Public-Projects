import onnx
m = onnx.load("model_fixed.onnx")
onnx.checker.check_model(m)
print("checker ok")