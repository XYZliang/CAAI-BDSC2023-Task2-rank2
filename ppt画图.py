import numpy as np
import matplotlib.pyplot as plt

plt.rcParams['font.sans-serif'] = ['SimHei']  # 指定默认字体为黑体
plt.rcParams['axes.unicode_minus'] = False
# 计算活跃度得分
# def compute_score(days):
#     return np.exp(-0.02 * days)
#
# # 时间范围（0-365天）
# days = np.arange(0, 366)
#
# # 计算得分
# scores = compute_score(days)
#
# # 创建图表
# plt.figure(figsize=(10, 6))
# plt.plot(days, scores, label="单次活跃度得分（w=0.02）")
# plt.title("活跃度得分随交互时间距数据集结束时间的间隔变大而衰减")
# plt.xlabel("时间间隔（天）")
# plt.ylabel("活跃度得分")
# plt.legend()
# plt.show()

import numpy as np
import matplotlib.pyplot as plt

import numpy as np
import matplotlib.pyplot as plt

# 随机生成100个点的交互商品个数和交互次数
np.random.seed(0)
interaction_counts = np.random.randint(1, 18, 60)
interaction_unique_counts = interaction_counts - np.random.randint(0, 15, 60)
interaction_unique_counts = np.maximum(interaction_unique_counts, 1)  # 确保交互商品个数至少为1

# 根据公式计算回流可能性得分，并进行对数转换
interaction_scores = np.exp((interaction_counts - interaction_unique_counts) / interaction_unique_counts)
interaction_scores = np.log1p(interaction_scores)  # 使用log1p函数进行对数转换，可以避免出现无穷大的情况

# 创建散点图
plt.figure(figsize=(10, 10))
plt.scatter(interaction_unique_counts, interaction_counts, c=interaction_scores, cmap='viridis')
plt.colorbar(label='回流可能性得分')
plt.xlabel('分享者和潜在回流者的交互商品个数')
plt.ylabel('分享者和潜在回流者的交互次数')
plt.title('用户交互商品重叠度得分关系图')
# 设定x和y轴的刻度间距
plt.xticks(np.arange(0, np.max(interaction_unique_counts) + 1, 2))
plt.yticks(np.arange(0, np.max(interaction_counts) + 5, 2))
plt.show()