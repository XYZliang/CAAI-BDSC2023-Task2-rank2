import pickle

import numpy as np
import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
from joblib import dump
from scipy.stats import gaussian_kde
import cloudpickle
import dill
from scipy.integrate import quad
from 分析用户活跃时间数据 import *


def makeKernel(df, title="User Action", show=True):
    # 创建镜像数据
    df_mirror1 = df.copy()
    df_mirror2 = df.copy()
    df_mirror1['timeByMin'] += 1440  # 添加24小时
    df_mirror2['timeByMin'] -= 1440  # 减去24小时
    # 合并所有数据
    df_all = pd.concat([df, df_mirror1, df_mirror2])

    # 创建一个默认带宽的KDE对象
    kde_default = gaussian_kde(df_all['timeByMin'])

    # 生成核密度估计
    kde = gaussian_kde(df_all['timeByMin'], bw_method=1 / 60)

    # 生成用于绘图的时间点
    minutes = np.linspace(0, 1440, 1440)

    # 计算各时间点的密度值
    density = kde(minutes)
    print(density)
    # 画图
    plt.figure(figsize=(10, 6))
    plt.plot(minutes / 60, density)
    plt.xlabel('Time (hours)')
    plt.ylabel('Density')
    plt.title('Kernel Density Estimate of ' + title)
    plt.grid(True)
    plt.ylim([0, 0.0008])
    if show:
        plt.show()
    plt.close()
    return kde


def process_group(group, n=3000, seed=2023):
    size = len(group)
    if size >= n:
        return group.sample(n=n, random_state=seed)
    elif n / 2 < size < n:
        sample = group.sample(n=int(n / 2), random_state=seed)
        return pd.concat([sample, sample.copy()], ignore_index=True)
    elif n / 3 <= size <= n / 2:
        sample = group.sample(n=int(n / 3), random_state=seed)
        return pd.concat([sample, sample.copy(), sample.copy()], ignore_index=True)
    elif n / 4 <= size < n / 3:
        sample = group.sample(n=int(n / 4), random_state=seed)
        return pd.concat([sample, sample.copy(), sample.copy()], ignore_index=True)
    elif n / 5 <= size < n / 4:
        sample = group.sample(n=int(n / 5), random_state=seed)
        return pd.concat([sample, sample.copy(), sample.copy()], ignore_index=True)
    else:
        m = 1.0 * n / size
        # 四舍五入
        m = int(m + 0.5) - 1
        return pd.concat([group] * m, ignore_index=True)


def getAKdePdf(df=pd.read_csv('./data/模型数据/df.csv'), isWorkDay=0):
    df = df[df['is_workday_or_holiday'] == isWorkDay]
    df = df.copy()
    # 如果包含action_taken列，则删除
    if 'action_taken' in df.columns:
        df = df[df['action_taken'] == 1]
    # 将小时、分钟和秒转换为以分钟为单位的时间
    df['timeByMin'] = df['hour'] * 60 + df['minute'] + df['second'] / 60
    # 删除列timestamp,year,month,day,hour,minute,second,date
    # if 'timestamp' in df.columns:  # 添加一个条件判断，只有当 'timestamp' 在列中时才进行删除
    #     df = df.drop(['timestamp'], axis=1)  # 使用返回的新 DataFrame 而不是原地修改
    if len(df) > 30000:
        df = df.sample(n=30000, random_state=2023)
    # 计算“平均”模型
    akde = makeKernel(df)
    # 定义概率密度函数
    pdf = lambda x: akde.evaluate(x)
    return pdf


def getPfromPdf(pdf, startTime=10 * 60 + 2, timeLength=5):
    # 计算概率
    prob, _ = quad(pdf, startTime, startTime + timeLength)
    return prob * 24 * (60 / timeLength)


def getDKdePdf(df=pd.read_csv('./data/模型数据/df.csv'), isWorkDay=0, userID=""):
    df = df[df['is_workday_or_holiday'] == isWorkDay]
    dfo = df.copy()
    df = df.copy()
    df = df[df['user_id'] == userID]
    # 如果包含action_taken列，则删除
    if 'action_taken' in df.columns:
        df = df[df['action_taken'] == 1]
    if 'action_taken' in dfo.columns:
        dfo = dfo[dfo['action_taken'] == 1]
    # 将小时、分钟和秒转换为以分钟为单位的时间
    df['timeByMin'] = df['hour'] * 60 + df['minute'] + df['second'] / 60
    # 删除列timestamp,year,month,day,hour,minute,second,date
    # if 'timestamp' in df.columns:  # 添加一个条件判断，只有当 'timestamp' 在列中时才进行删除
    #     df = df.drop(['timestamp'], axis=1)  # 使用返回的新 DataFrame 而不是原地修改
    # df = process_group(df)
    df = df[:1]
    df = pd.concat([df] * 100, ignore_index=True)
    if len(dfo) > 30000:
        dfo = dfo.sample(n=30000, random_state=2023)
    df_all = pd.concat([df, dfo])
    df_all['timeByMin'] = df_all['hour'] * 60 + df_all['minute'] + df_all['second'] / 60

    # 计算“平均”模型
    dkde = makeKernel(df_all, title=userID)
    # 定义概率密度函数
    pdf = lambda x: dkde.evaluate(x)
    return pdf


def initPD(historyDf):
    if "hour" not in historyDf.columns:
        historyDf['timestamp'] = pd.to_datetime(historyDf['timestamp'])
        historyDf['hour'] = historyDf['timestamp'].dt.hour
        historyDf['minute'] = historyDf['timestamp'].dt.minute
        historyDf['second'] = historyDf['timestamp'].dt.second
        historyDf = historyDf.drop(['timestamp'], axis=1)
    return historyDf


if __name__ == "__main__":
    df = pd.read_csv('./data/模型数据/user_history_False_False_False.json')
    df = initPD(df)
    a = getPfromPdf(getAKdePdf(df, isWorkDay=1), startTime=16 * 60 + 2, timeLength=5)
    # b = getPfromPdf(getDKdePdf(df, isWorkDay=1, userID="0005b30eb7d0182124c853809d726912"), startTime=16 * 60 + 2,
    #                 timeLength=5)
    # print("在21：02，用户的平均回流概率为" + str(a))
    # print("在21：02，该名用户的回流概率为" + str(b))
    # print("差值为" + str(a - b))
