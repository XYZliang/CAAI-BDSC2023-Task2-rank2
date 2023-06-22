import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns

# 读取 json 文件并转换为 DataFrame
df = pd.read_json('./data/item_share_train_info_AB.json')

# 在 user_id 和 item_id 一样的情况下，得到 voter_id 的数量
grouped_df = df.groupby(['inviter_id', 'item_id']).size().reset_index(name='voter_count')

# 输出 voter_id 的数量的平均数和中位数
print('Average number of voter_id:', grouped_df['voter_count'].mean())
print('Median number of voter_id:', grouped_df['voter_count'].median())

# # 绘制 voter_id 的数量的分布情况直方图和拟合曲线
# plt.figure(figsize=(10, 6))
# sns.histplot(grouped_df['voter_count'], kde=True)
# plt.xlabel('Number of voter_id')
# plt.ylabel('Frequency')
# plt.title('Distribution of the number of voter_id')
# # plt.show()
# # 绘制 voter_id 的数量的密度图
# plt.figure(figsize=(10, 6))
# sns.kdeplot(grouped_df['voter_count'], fill=True)
# plt.xlabel('Number of voter_id')
# plt.ylabel('Density')
# plt.title('Density plot of the number of voter_id')
# # plt.show()


def more(num):
    # 计算 voter_id 数量大于 3 的数量
    num_greate = grouped_df[grouped_df['voter_count'] > num].shape[0]
    print('用户分享某个商品的回流用户超过' + str(num) + '的个数:', num_greate)
    df_greater = grouped_df[grouped_df['voter_count'] > 3]
    result_list = list(df_greater.itertuples(index=False, name=None))[:5]
    print(result_list)


more(3)
more(5)
more(10)
more(20)
more(40)
more(60)
more(80)
more(85)

print()
# 按 user_id 进行分组，并计算每组的平均值和中位数
grouped_by_user = df.groupby('inviter_id').size()
user_mean = grouped_by_user.mean()
user_median = grouped_by_user.median()

print('Average voter count by user:', user_mean)
print('Median voter count by user:', user_median)

# # 绘制每个 user_id 的 voter_count 的分布的直方图
# plt.figure(figsize=(10, 6))
# sns.histplot(grouped_by_user, kde=False)
# plt.xlabel('Number of voter_id')
# plt.ylabel('Frequency')
# plt.title('Distribution of the number of voter_id by user')
# plt.show()
#
# # 绘制每个 user_id 的 voter_count 的密度图
# plt.figure(figsize=(10, 6))
# sns.kdeplot(grouped_by_user, fill=True)
# plt.xlabel('Number of voter_id')
# plt.ylabel('Density')
# plt.title('Density plot of the number of voter_id by user')
# plt.show()
# 计算组内数量大于1000的分组数量
num_groups_over_1000 = grouped_by_user[grouped_by_user > 1000].shape[0]
print('Number of groups with more than 1000 voter_id:', num_groups_over_1000)
num_groups_over_1000 = grouped_by_user[grouped_by_user > 100].shape[0]
print('Number of groups with more than 1000 voter_id:', num_groups_over_1000)
