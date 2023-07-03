import orjson
import pandas as pd

# 1. 读取json文件
with open('./data/item_info.json', 'rb') as f:
    item_info_data = orjson.loads(f.read())
item_info_df = pd.DataFrame(item_info_data)

with open('./data/item_share_train_info.json', 'rb') as f:
    train_info_data = orjson.loads(f.read())
train_info_df = pd.DataFrame(train_info_data)

# 2. 合并两个dataframe
df = pd.merge(train_info_df, item_info_df, on='item_id', how='left')

# 3. 统计商品id种类个数，叶子类目id种类数，一级类目id种类数，品牌id种类数，店铺id种类数
item_id_count = df['item_id'].nunique()
cate_id_count = df['cate_id'].nunique()
cate_level1_id_count = df['cate_level1_id'].nunique()
brand_id_count = df['brand_id'].nunique()
shop_id_count = df['shop_id'].nunique()


# 4. 统计各类别的id最多和最少的个数
def id_counts_stats(column_name):
    column_counts = df[column_name].value_counts()
    max_id = column_counts.idxmax()
    max_count = column_counts.max()
    min_id = column_counts.idxmin()
    min_count = column_counts.min()
    return max_id, max_count, min_id, min_count


item_id_stats = id_counts_stats('item_id')
cate_id_stats = id_counts_stats('cate_id')
cate_level1_id_stats = id_counts_stats('cate_level1_id')
brand_id_stats = id_counts_stats('brand_id')
shop_id_stats = id_counts_stats('shop_id')

# 输出统计结果
print(
    f'商品ID种类个数: {item_id_count},\n 叶子类目ID种类数: {cate_id_count},\n 一级类目ID种类数: {cate_level1_id_count},\n 品牌ID种类数: {brand_id_count},\n 店铺ID种类数: {shop_id_count}\n')
print(
    f'商品ID最多数量的ID及其个数: {item_id_stats},\n 叶子类目ID最多数量的ID及其个数: {cate_id_stats},\n 一级类目ID最多数量的ID及其个数: {cate_level1_id_stats},\n 品牌ID最多数量的ID及其个数: {brand_id_stats},\n 店铺ID最多数量的ID及其个数: {shop_id_stats}\n')

# 5. 根据商品的种类信息，可以考虑输出一些额外的统计数据，比如每个商品的平均分享次数等
avg_share_per_item = df['item_id'].value_counts().mean()
print(f'每个商品的平均分享次数: {avg_share_per_item}')


def id_less_than_5(column_name):
    column_counts = df[column_name].value_counts()
    less_than_5 = column_counts[column_counts < 10].count()
    ratio = less_than_5 / column_counts.count()
    return less_than_5, ratio


item_id_less_5_stats = id_less_than_5('item_id')
cate_id_less_5_stats = id_less_than_5('cate_id')
cate_level1_id_less_5_stats = id_less_than_5('cate_level1_id')
brand_id_less_5_stats = id_less_than_5('brand_id')
shop_id_less_5_stats = id_less_than_5('shop_id')

print(f'商品ID数量少于5的个数及其占比: {item_id_less_5_stats}')
print(f'叶子类目ID数量少于5的个数及其占比: {cate_id_less_5_stats}')
print(f'一级类目ID数量少于5的个数及其占比: {cate_level1_id_less_5_stats}')
print(f'品牌ID数量少于5的个数及其占比: {brand_id_less_5_stats}')
print(f'店铺ID数量少于5的个数及其占比: {shop_id_less_5_stats}')
