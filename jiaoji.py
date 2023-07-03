import orjson as json
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
from tqdm import tqdm

# 加载数据
with open('./data/item_share_train_info.json', 'r') as f:
    data = json.loads(f.read())

df = pd.DataFrame(data)

# 对于每个用户，得到他们所交互过的商品集合
user_items = df.groupby('inviter_id')['item_id'].apply(set)

from joblib import Parallel, delayed

def compute_intersection_and_union(user_items_chunk,iii,user_items):
    intersections = []
    unions = []
    for i in tqdm(range(len(user_items_chunk)),desc="线程"+str(iii),position=1):
        for j in range(i+1, len(user_items)):
            intersection = user_items[i] & user_items[j]
            union = user_items[i] | user_items[j]
            intersections.append(len(intersection))
            unions.append(len(union))
    return intersections, unions


# 将数据切割为100份
n_chunks = 100
chunks = [user_items[i::n_chunks] for i in range(n_chunks)]

# 使用joblib进行多进程处理
results = Parallel(n_jobs=-1)(delayed(compute_intersection_and_union)(chunks[i],i,user_items) for i in range(len(chunks)))

# 将结果合并
intersections, unions = zip(*results)
intersections = [item for sublist in intersections for item in sublist]
unions = [item for sublist in unions for item in sublist]

# 创建一个figure并添加两个子图
fig, axs = plt.subplots(2, figsize=(10, 10))

# 为每个子图绘制直方图
sns.histplot(intersections, color='blue', ax=axs[0])
sns.histplot(unions, color='red', ax=axs[1])

# 添加标题
axs[0].set_title('Intersection Distribution')
axs[1].set_title('Union Distribution')

plt.tight_layout()
plt.show()
