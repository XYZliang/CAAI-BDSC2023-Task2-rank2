import orjson as json
import pandas as pd
from pathlib import Path

data_dir = Path("./data")


# 1. 读取数据
def read_json(file_path):
    with open(file_path, 'r') as f:
        data = json.loads(f.read())
    return pd.DataFrame(data)


user_info_df = read_json(data_dir / 'user_info.json')
item_share_train_info_df = read_json(data_dir / 'item_share_train_info.json')

# 2. 统计user_info.json中，user_gender、user_age、user_level的各个类别的数量（不包含未知的-1）及占比
user_info_df = user_info_df[
    (user_info_df.user_gender != -1) & (user_info_df.user_age != -1) & (user_info_df.user_level != -1)]

gender_counts = user_info_df.user_gender.value_counts()
gender_freq = user_info_df.user_gender.value_counts(normalize=True)
age_counts = user_info_df.user_age.value_counts()
age_freq = user_info_df.user_age.value_counts(normalize=True)
level_counts = user_info_df.user_level.value_counts()
level_freq = user_info_df.user_level.value_counts(normalize=True)

# 3. 对于item_share_train_info.json中的所有voter_id，统计这些用户的user_gender、user_age、user_level的各个类别的数量（不包含未知的-1）及占比
# 连接 user_info_df 和 item_share_train_info_df
merged_df = pd.merge(item_share_train_info_df, user_info_df, left_on='voter_id', right_on='user_id', how='left')

# 对 voter 的 user_gender、user_age、user_level 的各个类别进行统计（不包含未知的-1）
merged_df = merged_df[(merged_df.user_gender != -1) & (merged_df.user_age != -1) & (merged_df.user_level != -1)]

voters_gender_counts = merged_df.user_gender.value_counts()
voters_gender_freq = merged_df.user_gender.value_counts(normalize=True)
voters_age_counts = merged_df.user_age.value_counts()
voters_age_freq = merged_df.user_age.value_counts(normalize=True)
voters_level_counts = merged_df.user_level.value_counts()
voters_level_freq = merged_df.user_level.value_counts(normalize=True)

# 4. 计算在每个特征的每一个类目下，在item_share_train_info.json出现的比例
voters_gender_proportion = voters_gender_counts / gender_counts
voters_age_proportion = voters_age_counts / age_counts
voters_level_proportion = voters_level_counts / level_counts

# 5. 输出统计结果
for feature in ['性别', '年龄段', '用户等级']:
    if feature == '性别':
        total_counts, total_freq, voters_counts, voters_freq, proportion = gender_counts, gender_freq, voters_gender_counts, voters_gender_freq, voters_gender_proportion
    elif feature == '年龄段':
        total_counts, total_freq, voters_counts, voters_freq, proportion = age_counts, age_freq, voters_age_counts, voters_age_freq, voters_age_proportion
    else:  # feature == '用户等级'
        total_counts, total_freq, voters_counts, voters_freq, proportion = level_counts, level_freq, voters_level_counts, voters_level_freq, voters_level_proportion

    result_df = pd.concat([total_counts, total_freq, voters_counts, voters_freq, proportion], axis=1,
                          keys=['总数', '总数占比', 'item_share总数', 'item_share总数占比', 'item_share比例'])
    print(f"{feature}统计结果：\n", result_df.to_string())
