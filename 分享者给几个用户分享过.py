import orjson
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
from scipy.stats import norm
import matplotlib.pyplot as plt
plt.rcParams['font.sans-serif'] = ['SimHei']  # SimHei字体是一个常见的支持中文的字体
plt.rcParams['axes.unicode_minus'] = False  # 解决保存图像是负号'-'显示为方块的问题

# 加载数据
with open('./data/item_share_train_info.json', 'rb') as f:
    data = orjson.loads(f.read())

# 将数据转化为DataFrame
df = pd.DataFrame(data)

# 统计每个邀请者的回流者数量
inviter_voter_counts = df.groupby('inviter_id')['voter_id'].nunique()

# 绘制分布图
plt.figure(figsize=(10, 6))
sns.distplot(inviter_voter_counts, kde=False, fit=norm)
plt.title('每个邀请者的回流者数量分布')
plt.xlabel('回流者数量')
plt.ylabel('频率')
plt.show()
