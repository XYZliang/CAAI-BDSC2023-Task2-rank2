# 读取./data/模型数据/df.csv到df
import pandas as pd

df = pd.read_csv('./data/模型数据/df.csv')
# 过滤action_taken为1的数据
df = df[df['action_taken'] == 1]

# 按照user_id进行分组
groups = df.groupby('user_id')

# 计算分组后组内元素数量大于0个的分组数量
count = sum([1 for _, group in groups if len(group) > 0])
print(f"分组后组内元素数量大于0个的分组数量: {count}")
# 输出几个分组的数据
for _, group in groups:
    if len(group) > 0:
        print(group)
        print()
        break
# 计算分组后组内元素数量为1个的分组数量
count = sum([1 for _, group in groups if len(group) == 1])
print(f"分组后组内元素数量1个的分组数量: {count}")
for _, group in groups:
    if len(group) > 1:
        print(group)
        print()
        break
# 计算分组后组内元素数量大于3个的分组数量
count = sum([1 for _, group in groups if len(group) > 3])
print(f"分组后组内元素数量大于3个的分组数量: {count}")
for _, group in groups:
    if len(group) > 3:
        print(group)
        print()
        break
# 计算分组后组内元素数量大于6个的分组数量
count = sum([1 for _, group in groups if len(group) > 6])
print(f"分组后组内元素数量大于6个的分组数量: {count}")
for _, group in groups:
    if len(group) > 6:
        print(group)
        print()
        break
# 计算分组后组内元素数量大于10个的分组数量
count = sum([1 for _, group in groups if len(group) > 10])
print(f"分组后组内元素数量大于10个的分组数量: {count}")
for _, group in groups:
    if len(group) > 10:
        print(group)
        print()
        break
# 计算分组后组内元素数量大于3个小于10个的分组数量
count = sum([1 for _, group in groups if 3 < len(group) < 10])
print(f"分组后组内元素数量大于3个小于10个的分组数量: {count}")
for _, group in groups:
    if 3 < len(group) < 10:
        print(group)
        print()
        break
# 计算分组后组内元素数量大于100个的分组数量
count = sum([1 for _, group in groups if len(group) > 100])
print(f"分组后组内元素数量大于100个的分组数量: {count}")
for _, group in groups:
    if len(group) > 100:
        print(group)
        print()
        break
# 计算分组后组内元素数量大于10个小于100个的分组数量
count = sum([1 for _, group in groups if 10 < len(group) < 100])
print(f"分组后组内元素数量大于10个小于100个的分组数量: {count}")
for _, group in groups:
    if 10 < len(group) < 100:
        print(group)
        print()
        break
# 计算分组后组内元素数量大于100个小于500个的分组数量
count = sum([1 for _, group in groups if 100 < len(group) < 500])
print(f"分组后组内元素数量大于100个小于500个的分组数量: {count}")
for _, group in groups:
    if 100 < len(group) < 500:
        print(group)
        print()
        break
# 计算分组后组内元素数量大于1000个的分组数量
count = sum([1 for _, group in groups if len(group) > 1000])
print(f"分组后组内元素数量大于1000个的分组数量: {count}")
for _, group in groups:
    if len(group) > 1000:
        print(group)
        print()
        break
print()
unique_user_id_count = df['user_id'].nunique()
print(f"'user_id'的种类个数: {unique_user_id_count}")
average_group_size = df.groupby('user_id').size().mean()
print(f"'user_id'分组的组内元素数量平均值: {average_group_size}")
median_group_size = df.groupby('user_id').size().median()
print(f"'user_id'分组的组内元素数量中位数: {median_group_size}")
max_group_size = df.groupby('user_id').size().max()
print(f"'user_id'分组的组内元素数量最大值: {max_group_size}")
