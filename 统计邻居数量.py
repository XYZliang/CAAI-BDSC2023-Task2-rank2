import matplotlib.pyplot as plt
plt.rcParams['font.sans-serif'] = ['SimHei']  # 指定默认字体为黑体
plt.rcParams['axes.unicode_minus'] = False
# 数据
hops = ['1 hop', '2 hops', '3 hops']
nodes = [96299, 10494, 6853]

# 创建条形图
bars = plt.bar(hops, nodes, color='blue')

# 添加每个柱形的数值
for bar in bars:
    yval = bar.get_height()
    plt.text(bar.get_x() + bar.get_width()/2, yval + 500, yval, ha='center', va='bottom')

# 添加标题和轴标签
plt.title('少于5个邻居的节点数量')
plt.xlabel('阶层数')
plt.ylabel('用户数', labelpad=0)

# 展示图像
plt.show()
