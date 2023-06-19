import matplotlib.pyplot as plt
import numpy as np

# 定义常数
a = 1
b = 0.004
e = np.exp(1)  # 自然对数的底数

# 定义x的范围
x = np.linspace(0, 1000, 4000)
y = (a + e ** (-b * x)) ** 5 * 4

plt.figure()
plt.plot(x, y)
plt.title('Graph of y = (a + e) ** (-b*x)')
plt.xlabel('x')
plt.ylabel('y')
plt.grid(True)
plt.show()
x =3
print((a + e ** (-b * x)) ** 5 * 4)
x = 400
print((a + e ** (-b * x)) ** 5 * 4)