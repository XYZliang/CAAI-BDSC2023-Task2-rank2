import orjson as json
# 读取 ./data/item_share_train_info.json和item_share_train_info_B.json
with open('./data/item_share_train_info.json', 'rb') as f:
    data = json.loads(f.read())
with open('./data/item_share_train_info_B.json', 'rb') as f:
    dataB = json.loads(f.read())
# 合并
data.extend(dataB)
# 保存
with open('./data/item_share_train_info_AB.json', 'wb') as f:
    f.write(json.dumps(data))
