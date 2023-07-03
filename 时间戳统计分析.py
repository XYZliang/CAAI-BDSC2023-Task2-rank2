import orjson
import pandas as pd
import matplotlib.pyplot as plt
import os
from tqdm import tqdm

# 读取JSON文件
with open('./data/item_share_test_info_B.json', 'rb') as f:
    data = orjson.loads(f.read())

# 将数据转换为Pandas DataFrame
df = pd.json_normalize(data)

# 将timestamp列转换为datetime类型
df['timestamp'] = pd.to_datetime(df['timestamp'])

# 添加额外的列来容纳年，月和日
df['year'] = df['timestamp'].dt.year
df['month'] = df['timestamp'].dt.month
df['day'] = df['timestamp'].dt.day
df['hour'] = df['timestamp'].dt.hour

# 计算每月的行为数量
monthly_counts = df.groupby(['year', 'month']).size()
fig, ax = plt.subplots(figsize=(len(monthly_counts), 6))
monthly_counts.plot(kind='bar', ax=ax)
plt.title('Monthly Behavior Counts')
plt.ylabel('Count')
plt.tight_layout()
plt.savefig('monthly_counts.png')  # Save the figure to a file
plt.close(fig)  # Close the figure

# 创建用于保存日行为数量图的文件夹
if not os.path.exists('daily_counts'):
    os.makedirs('daily_counts')

# 按月导出每日的行为数量
for name, group in tqdm(df.groupby(['year', 'month']), desc="Processing daily counts"):
    daily_counts = group.groupby(['day']).size()
    fig, ax = plt.subplots(figsize=(len(daily_counts) / 4, 6))
    daily_counts.plot(kind='bar', ax=ax)
    plt.title('Daily Behavior Counts for ' + str(name))
    plt.ylabel('Count')
    plt.tight_layout()
    plt.savefig(f'daily_counts/daily_counts_{name}.png')
    plt.close(fig)  # Close the figure

# 创建用于保存小时行为数量图的文件夹
if not os.path.exists('hourly_counts'):
    os.makedirs('hourly_counts')

# 按天导出每小时的行为数量
for name, group in tqdm(df.groupby(['year', 'month', 'day']), desc="Processing hourly counts"):
    hourly_counts = group.groupby(['hour']).size()
    fig, ax = plt.subplots(figsize=(24, 6))  # Increase figure size
    hourly_counts.plot(kind='bar', ax=ax)
    plt.title('Hourly Behavior Counts for ' + str(name))
    plt.ylabel('Count')
    plt.xticks(rotation=45)  # Rotate x-axis labels
    plt.tight_layout()
    plt.savefig(f'hourly_counts/hourly_counts_{name}.png')
    plt.close(fig)  # Close the figure
