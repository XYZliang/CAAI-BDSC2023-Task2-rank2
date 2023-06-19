import numpy as np
import matplotlib.pyplot as plt

def custom_sigmoid(x, scale):
    y = 1 / (1 + np.exp(-scale * x)) - 1 / (1 + np.exp(0))
    return y / (1 - 1 / (1 + np.exp(0)))

# 创建一个x值的范围
x = 4

# 计算对应的y值
y = custom_sigmoid(x, 0.25)
print(y)
# 使用matplotlib来画出函数的图像
plt.plot(x, y)
plt.title("Custom Sigmoid Function")
plt.xlabel("x")
plt.ylabel("y")
plt.grid(True)
plt.show()