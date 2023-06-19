import numpy as np
import psutil

def print_available_memory():
    mem = psutil.virtual_memory()
    print('当前可用内存: ', mem.available / 1024 ** 3, 'GB')

print_available_memory()

try:
    # 尝试创建一个11万*11万的数组，数据类型为64位浮点数
    arr = np.zeros((110000, 110000), dtype=np.float64)
    print("数组创建成功。")
except MemoryError:
    print("内存不足，无法创建数组。")

print_available_memory()
