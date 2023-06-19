import orjson as json
import numpy as np


def split_dataset(file_path, train_ratio=0.9):
    with open(file_path, 'rb') as f:
        data = json.loads(f.read())

    # 计算训练集大小
    train_size = int(train_ratio * len(data))

    # 分割数据
    train_data = data[:train_size]
    test_data = data[train_size:]
    return train_data, test_data


train_data, test_data = split_dataset('./data/item_share_train_info.json')

# 保存分割后的训练集和测试集到json文件
with open('./data/item_share_train_info_09.json', 'wb') as f:
    f.write(json.dumps(train_data))

def transform_data(test_data):
    # 转换测试集数据格式
    transformed_data = []
    for i, record in enumerate(test_data):
        transformed_record = {
            'triple_id': str(i).zfill(6),
            'inviter_id': record['inviter_id'],
            'item_id': record['item_id'],
            'timestamp': record['timestamp'],
        }
        transformed_data.append(transformed_record)
    return transformed_data

def transform_dataa(test_data):
    # 转换测试集数据格式
    transformed_data = []
    for i, record in enumerate(test_data):
        transformed_record = {
            'triple_id': str(i).zfill(6),
            'voter_id': record['voter_id'],
            'inviter_id': record['inviter_id'],
            'item_id': record['item_id'],
            'timestamp': record['timestamp'],
        }
        transformed_data.append(transformed_record)
    return transformed_data

# 保存转换后的测试集到json文件
with open('./data/item_share_train_info_test_01_ans.json', 'wb') as f:
    f.write(json.dumps(transform_dataa(test_data)))

transformed_test_data = transform_data(test_data)

# 保存转换后的测试集到json文件
with open('./data/item_share_train_info_test_01.json', 'wb') as f:
    f.write(json.dumps(transformed_test_data))


def count_unseen_inviters(train_data, test_data):
    # 创建一个集合存储训练集中的所有inviter_id
    train_inviters = set(record['inviter_id'] for record in train_data)

    unseen_count = 0
    for record in test_data:
        if record['inviter_id'] not in train_inviters:
            unseen_count += 1

    return unseen_count
unseen_count = count_unseen_inviters(train_data, transformed_test_data)
print(len(transformed_test_data))
print(f'在测试集中，有{unseen_count}个inviter_id没有在训练集中出现过。')
print(unseen_count/len(transformed_test_data))
with open('./data/item_share_train_info.json', 'rb') as f:
    train_data = json.loads(f.read())

with open('./data/item_share_preliminary_test_info.json', 'rb') as f:
    transformed_test_data = json.loads(f.read())

unseen_count = count_unseen_inviters(train_data, transformed_test_data)
print(f'在测试集中，有{unseen_count}个inviter_id没有在训练集中出现过。')
print(unseen_count/len(transformed_test_data))
print(len(transformed_test_data))
