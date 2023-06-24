import pandas as pd

# 读取 json 文件并转换为 DataFrame
df = pd.read_json('./data/item_share_train_info.json')

# 找到没有在 voter_id 中出现的 inviter_id
unique_inviter_ids = df[~df['inviter_id'].isin(df['voter_id'])]['inviter_id'].unique()

# 按 inviter_id 进行分组，并计算每个分组的大小
grouped_df = df[df['inviter_id'].isin(unique_inviter_ids)].groupby('inviter_id').size().reset_index(name='group_size')

# 转换为 list of tuples，每个 tuple 包含 (inviter_id, group_size)
result_list = list(grouped_df.itertuples(index=False, name=None))

import matplotlib.pyplot as plt
import seaborn as sns

# 计算 result_list 中分组大小的平均数和中位数
mean_size = grouped_df['group_size'].mean()
median_size = grouped_df['group_size'].median()

print(len(result_list))
print('Average group size:', mean_size)
print('Median group size:', median_size)

# # 绘制分组大小的分布情况直方图
# plt.figure(figsize=(10, 6))
# sns.histplot(grouped_df['group_size'], kde=False)
# plt.xlabel('Group Size')
# plt.ylabel('Frequency')
# plt.title('Distribution of Group Size')
# plt.show()
#
# # 绘制分组大小的密度图
# plt.figure(figsize=(10, 6))
# sns.kdeplot(grouped_df['group_size'], fill=True)
# plt.xlabel('Group Size')
# plt.ylabel('Density')
# plt.title('Density Plot of Group Size')
# plt.show()

# 将 result_list 按照 group_size 从大到小排序
sorted_list = sorted(result_list, key=lambda x: x[1], reverse=True)

# 计算前 10% 的元素的数量
num_top_10_percent = int(len(sorted_list) * 0.1)

# 取前 10% 的元素
top_10_percent = sorted_list[:num_top_10_percent]

# 找到前 10% 的元素中最小的那个
min_top_10_percent = min(top_10_percent, key=lambda x: x[1])

print('Top 10% elements:', top_10_percent)
print('Minimum in top 10%:', min_top_10_percent)
