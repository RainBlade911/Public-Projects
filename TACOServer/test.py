import onnxruntime as rt

providers = rt.get_all_providers()
print(providers)