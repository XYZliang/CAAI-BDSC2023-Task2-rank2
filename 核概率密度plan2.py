import gc
import math

import numpy as np
import matplotlib.pyplot as plt
import matplotlib as mpl

import kalepy as kale
import pandas as pd

from kalepy.plot import nbshow
import numpy as np
import matplotlib.pyplot as plt
import kalepy as kale
from scipy.interpolate import interp1d


def makeSampleData(df):
    samples = df['timeByMin'].values
    # 创建三个版本的样本数组
    samples_mirror1 = samples - 1440
    # 截取大于-720的部分
    samples_mirror1 = samples_mirror1[samples_mirror1 > -120]
    samples_mirror2 = samples + 1440
    # 截取小于720的部分
    samples_mirror2 = samples_mirror2[samples_mirror2 < 1440 + 120]
    # 将三个数组合并成一个
    samples_all = np.concatenate([samples, samples_mirror1, samples_mirror2])
    weights = np.ones(len(samples_all))
    weights = weights / sum(weights)
    return samples_all, weights


# 输出np数组的平均值、标准差
def printMeanAndStd(samples):
    print("mean:" + str(np.mean(samples)))
    print("std:" + str(np.std(samples)))


def makeDiySampleData(dfs, dfu):
    num = len(dfs)
    diyNum = len(dfu)
    # 定义常数
    a = 1
    b = 0.004
    e = np.exp(1)  # 自然对数的底数
    UW = (a + e ** (-b * diyNum)) ** 5 * 4
    # print("UW"+str(UW))
    # 使用merge函数找到两个DataFrame中相同的部分
    common = dfs.merge(dfu, how='inner')
    # 从dfs中删除在dfu中也存在的行
    dfs = dfs[(~dfs.isin(common))].dropna()
    dfs = dfs.sample(n=num - diyNum, random_state=2023)
    weights = np.ones(num - diyNum)
    df = pd.concat([dfs, dfu], ignore_index=True)
    weightsD = np.ones(diyNum) * UW
    samples = df['timeByMin'].values
    # makeSampleData(samples)
    # 创建三个版本的样本数组
    samples_mirror1 = samples - 1440
    # 截取大于-720的部分
    samples_mirror1 = samples_mirror1[samples_mirror1 > -120]
    samples_mirror2 = samples + 1440
    # 截取小于720的部分
    samples_mirror2 = samples_mirror2[samples_mirror2 < 1440 + 120]
    # 将三个数组合并成一个
    samples_all = np.concatenate([samples, samples_mirror1, samples_mirror2])
    weightsss = np.concatenate([weights, weightsD, np.ones(len(samples_all) - num)])
    weightsss = weightsss / sum(weightsss)
    return samples_all, weightsss


# 计算
def getP(points, density, start_time):
    interp_density = interp1d(points, density)
    # 创建一个时间范围
    # time_range = np.linspace(start_time, end_time, 60*timeLength)  # 可以调整最后一个参数以改变时间范围的精度
    # 计算每个时间点的密度
    density_range = interp_density(start_time)
    # print(density_range)
    # 计算总概率
    # total_probability = np.sum(density_range) * (end_time - start_time) / len(time_range)
    # # 映射到全局概率
    # total_probability = total_probability
    # print(f"The probability of event A happening between {start_time} and {end_time} is {total_probability}")
    return density_range


def getAKdePdf(df=pd.read_csv('./data/模型数据/df.csv'), isWorkDay=0, BD=1 / 20, show=False):
    df = df.copy()
    df = initPD(df)
    df = df[df['is_workday_or_holiday'] == isWorkDay]
    # 如果包含action_taken列，则删除
    if 'action_taken' in df.columns:
        df = df[df['action_taken'] == 1]
    # 将小时、分钟和秒转换为以分钟为单位的时间
    if len(df) > 30000:
        df = df.sample(n=30000, random_state=2023)
    df['timeByMin'] = df['hour'] * 60 + df['minute'] + df['second'] / 60
    samples_all, weights = makeSampleData(df)
    points, density = kale.density(samples_all, weights=weights, probability=True, bandwidth=BD, reflect=True)
    if show:
        # 绘制核密度估计
        plt.figure(figsize=(10, 6))
        plt.plot(points / 60, density * 1.1, 'k-', lw=2.0, alpha=0.8, label='KDE')
        plt.ylim([0, 0.0015])
        plt.xlim([0, 24])
        plt.xlabel('timeByHour for ALL')
        plt.ylabel('Density')
        plt.legend()
        plt.show()
    return [points, density]


def getPfromPdf(pdf, startTime=10 * 60 + 2):
    points = pdf[0]
    density = pdf[1]
    # 计算概率
    density_range = getP(points, density, startTime)
    return density_range


def getDKdePdf(df=pd.read_csv('./data/模型数据/df.csv'), isWorkDay=0, userID="", BD=1 / 20, show=False):
    df = df.copy()
    df = initPD(df)
    df = df[df['is_workday_or_holiday'] == isWorkDay]
    # 如果包含action_taken列，则删除
    if 'action_taken' in df.columns:
        df = df[df['action_taken'] == 1]
    if len(df) > 30000:
        dfs = df.sample(n=30000, random_state=2023)
    dfu = df[df['user_id'] == userID]
    samples_new, weights_new = makeDiySampleData(dfs, dfu)
    Npoints, Ndensity = kale.density(samples_new, weights=weights_new, probability=True, bandwidth=BD, reflect=True)
    if show:
        # 绘制核密度估计
        plt.figure(figsize=(10, 6))
        plt.plot(Npoints / 60, Ndensity * 1.1, 'k-', lw=2.0, alpha=0.8, label='NKDE')
        plt.ylim([0, 0.0015])
        plt.xlim([0, 24])
        plt.xlabel('timeByHour for ' + str(userID))
        plt.ylabel('Density')
        plt.legend()
        plt.show()
    return [Npoints, Ndensity]


def initPD(historyDf):
    if "hour" not in historyDf.columns:
        historyDf['timestamp'] = pd.to_datetime(historyDf['timestamp'])
        historyDf['hour'] = historyDf['timestamp'].dt.hour
        historyDf['minute'] = historyDf['timestamp'].dt.minute
        historyDf['second'] = historyDf['timestamp'].dt.second
        historyDf = historyDf.drop(['timestamp'], axis=1)
    return historyDf


def getMultiplier(opdf=None, updf=None, time=21 * 60 + 2, show=False):
    if opdf is None:
        a = 1.0
    else:
        a = getPfromPdf(opdf, time)
    b = getPfromPdf(updf, time)
    c = (b / a) ** 2
    if show:
        print(c)
        bouth(opdf, updf)
    return c


def bouth(opdf=None, updf=None):
    points = opdf[0]
    density = opdf[1]
    Npoints = updf[0]
    Ndensity = updf[1]
    plt.figure(figsize=(10, 6))
    plt.plot(points / 60, density * 1.1, lw=2.0, alpha=0.8, label='ALL KDE', color="red")
    plt.plot(Npoints / 60, Ndensity * 1.1, lw=2.0, alpha=0.8, label='USER KDE', color="deepskyblue")
    plt.ylim([0, 0.0015])
    plt.xlim([0, 24])
    plt.xlabel('timeByHour')
    plt.ylabel('Density')
    plt.legend()
    plt.show()


if __name__ == "__main__":
    df = pd.read_csv('./data/模型数据/user_history_False_False_False.json')
    apdf = getAKdePdf(df, 0, 1 / 20, True)
    dpdf = getDKdePdf(df, 0, "0005b30eb7d0182124c853809d726912", 1 / 20, True)
    getMultiplier(apdf, dpdf, 21 * 60 + 2, True)
