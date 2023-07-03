import orjson
import pandas as pd
import matplotlib.pyplot as plt
plt.rcParams['font.sans-serif'] = ['SimHei']  # 指定默认字体为黑体
plt.rcParams['axes.unicode_minus'] = False
from joblib import Parallel, delayed
from scipy.stats import gaussian_kde
import numpy as np
from tqdm import tqdm

# 读取json文件并转换为DataFrame
with open('./data/item_share_train_info.json', 'rb') as f:
    data = orjson.loads(f.read())
df = pd.DataFrame(data)

# 统计作为分享者和回流者的次数
inviter_counts = df['inviter_id'].value_counts()
voter_counts = df['voter_id'].value_counts()

# 分别对两者的分布进行绘图并进行函数拟合
def plot_distribution_and_fit(counts, title):
    density = gaussian_kde(counts)
    x = np.linspace(min(counts), max(counts), 1000)
    plt.plot(x, density(x), label='密度')
    plt.hist(counts, density=True, bins=30, alpha=0.5, label='直方图')
    plt.title(title)
    plt.legend()
    plt.show()

plot_distribution_and_fit(inviter_counts, '分享者所有分享的回流用户次数')
plot_distribution_and_fit(voter_counts, 'Distribution of Counts for Each Voter')
#
# import networkx as nx
#
# # 创建一个无向图
# G = nx.Graph()
# # 将df的数据加入到图中
# edges = list(zip(df['inviter_id'], df['voter_id']))
# G.add_edges_from(edges)
# # 切分节点
# nodes = list(G.nodes())
# chunks = [nodes[i::100] for i in range(100)]
#
# def process_chunk(chunk,i):
#     # 这是每个线程需要执行的函数，它接收一个节点的子集，并计算它们的邻居数量
#     results = []
#     for node in tqdm(chunk,desc="线程"+str(i),position=1):
#         neighbors = nx.single_source_shortest_path_length(G, node, cutoff=999)
#         results.append((node, len(neighbors)))
#     return results
#
# # 使用joblib进行并行处理
# results = Parallel(n_jobs=-1)(delayed(process_chunk)(chunks[i],i) for i in tqdm(range(len(chunks))))
#
# # 整理结果
# results = [item for sublist in results for item in sublist] # flatten list of lists
# results = pd.DataFrame(results, columns=['node', 'neighbor_count'])
#
# # for i in range(1, 4):
# #     print(f"Number of nodes with less than 5 neighbors within {i} hops: {count_neighbors_within_n_hops(G, i)}")
