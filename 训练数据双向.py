import orjson

# 读取原始json文件
with open('./data/item_share_train_info.json', 'rb') as f:
    data = orjson.loads(f.read())

new_data = []
# 对每一个json对象，调换inviter_id和voter_id的位置
for item in data:
    new_item = item.copy()  # 创建一个新的字典，以避免修改原始数据
    new_item['inviter_id'], new_item['voter_id'] = item['voter_id'], item['inviter_id']
    new_data.append(new_item)

# 将新的json数组追加到原始json数组后面
data.extend(new_data)

# 将修改后的json数组写入到新的json文件中
with open('./data/item_share_train_info_new.json', 'wb') as f:
    f.write(orjson.dumps(data))
