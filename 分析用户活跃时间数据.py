import os
import random

import numpy as np
import orjson
import pandas as pd
import psutil
from joblib import Parallel, delayed
from tqdm import tqdm
from chinese_calendar import is_workday
from joblib import Parallel, delayed


def getThreading(Hyperthreading=False, MaxThreading=0, hard=1, mem=1.2):
    # 获取用户单CPU的核心数、线程数、内存数
    cpu_count = psutil.cpu_count(False)

    cpu_count_logical = psutil.cpu_count(True)
    free_memory = psutil.virtual_memory().available / 1024 / 1024 / 1024
    # 保留两位小数
    free_memory = round(free_memory, 2)
    # 初始化进程
    if Hyperthreading:
        threadingNum = round(cpu_count_logical)
    else:
        threadingNum = round(cpu_count)
    memNum = round(free_memory / mem)
    cpuNum = round(threadingNum / hard)
    threadingNum = min(memNum, cpuNum)
    if MaxThreading != 0:
        if threadingNum > MaxThreading:
            threadingNum = MaxThreading
    if threadingNum > cpu_count_logical:
        threadingNum = cpu_count_logical
    print("CPU核心数：", cpu_count)
    print("可用内存：", free_memory, "GB")
    print("线程数：", threadingNum)
    return threadingNum


# 为每个用户生成一些随机时间
def generate_random_timestamps(n, year, month=None, day=None):
    if month is None:
        month = np.random.randint(1, 13)  # random month
    if day is None:
        day = np.random.randint(1, 29)  # random day, to be safe with February

    start_time = pd.Timestamp(f"{year}-{month:02d}-{day:02d} 00:00:00")
    end_time = pd.Timestamp(f"{year}-{month:02d}-{day:02d} 23:59:59")

    return pd.to_datetime(np.random.randint(start_time.value // 10 ** 9,
                                            end_time.value // 10 ** 9,
                                            size=n), unit='s')


def generate_negative_samples_for_users(i, historyDf):
    negative_samples = []
    pbar = tqdm(historyDf.iterrows(), total=len(historyDf), position=1, desc="进程" + str(i) + "正在运行")

    for _, action in pbar:
        user = action['user_id']
        year, month, day = action['timestamp'].year, action['timestamp'].month, action['timestamp'].day
        num_positive_samples = 1
        hisTime = user_history[user]
        plan = random.randint(0, 1)

        if plan == 0:
            random_timestamp1 = generate_random_timestamps(num_positive_samples, year, month, day)
            random_timestamp1 = random_timestamp1[~random_timestamp1.isin(hisTime)]

            time1 = 1
            while len(random_timestamp1) < num_positive_samples:
                if time1 >= 6:
                    break
                additional_samples_needed = num_positive_samples - len(random_timestamp1)
                new_random_timestamps = generate_random_timestamps(additional_samples_needed * time1, year, month, day)
                new_random_timestamps = random_timestamp1[
                    ~random_timestamp1.isin(historyDf.loc[historyDf['user_id'] == user, 'timestamp'])]
                new_random_timestamps = new_random_timestamps[:num_positive_samples]
                random_timestamp1 = random_timestamp1.append(new_random_timestamps)
                time1 += 1

            if len(random_timestamp1) != 0:
                negative_samples.append({
                    'user_id': user,
                    'timestamp': random_timestamp1[0],
                    'is_workday_or_holiday': 0 if is_workday(random_timestamp1[0]) else 1
                })


        else:
            random_timestamp2 = generate_random_timestamps(num_positive_samples, year)
            random_timestamp2 = random_timestamp2[~random_timestamp2.isin(hisTime)]

            time2 = 1
            while len(random_timestamp2) < num_positive_samples:
                if time2 >= 6:
                    break
                additional_samples_needed = num_positive_samples - len(random_timestamp2)
                new_random_timestamps = generate_random_timestamps(additional_samples_needed * time2, year)
                new_random_timestamps = random_timestamp2[
                    ~random_timestamp2.isin(historyDf.loc[historyDf['user_id'] == user, 'timestamp'])]
                new_random_timestamps = new_random_timestamps[:num_positive_samples]
                random_timestamp2 = random_timestamp2.append(new_random_timestamps)
                time2 += 1

            if len(random_timestamp2) != 0:
                negative_samples.append({
                    'user_id': user,
                    'timestamp': random_timestamp2[0],
                    'is_workday_or_holiday': 0 if is_workday(random_timestamp2[0]) else 1
                })
    negative_samples = pd.DataFrame(negative_samples)
    return negative_samples


def process_users(i, users, df):
    return {user: df.loc[df['user_id'] == user, 'timestamp'].dt.strftime('%Y-%m-%d %H:%M:%S').tolist() for user in
            tqdm(users, position=1, desc="进程" + str(i) + "预处理中")}


def split_list(lst, n):
    # 分割列表为 n 个均匀的部分
    division = len(lst) // n
    return [lst[i:i + division] for i in range(0, len(lst), division)]


def get_user_history_parallel(df):
    num_cores = getThreading(hard=1.2, mem=0.5)  # 使用所有可用的核心
    unique_users = df['user_id'].unique()
    split_users = split_list(unique_users, int(num_cores * num_cores / 2))
    results = Parallel(n_jobs=num_cores)(delayed(process_users)(i, split_users[i], df) for i in range(len(split_users)))
    # 合并所有的字典结果
    user_history = {k: v for res in results for k, v in res.items()}
    return user_history


def load_user_history(filename='./data/模型数据/user_history.json'):
    with open(filename, 'rb') as f:
        return orjson.loads(f.read())


def save_user_history(user_history, filename='./data/模型数据/user_history.json'):
    with open(filename, 'wb') as f:
        f.write(orjson.dumps(user_history))


def get_active_data(data=None, needUserInfo=False, needNative=False, full=False):
    if data is None:
        # 读取 历史行为数据文件
        with open('./data/item_share_train_info.json', 'rb') as f:
            data = orjson.loads(f.read())

    # 将数据转换为 DataFrame
    historyDf = pd.DataFrame(data)

    # # 取前25%的数据
    # historyDf = historyDf.head(int(len(historyDf) * 0.25))

    # # # 取前100行debug
    # historyDf = historyDf.head(16000)

    # 删除inviter_id item_id
    historyDf.drop(['inviter_id', 'item_id'], axis=1, inplace=True)
    historyDf = historyDf.rename(columns={'voter_id': 'user_id'})

    # 将 timestamp 列转换为 datetime 类型
    historyDf['timestamp'] = pd.to_datetime(historyDf['timestamp'])

    # 提取并添加年、月、日、小时、分钟的列
    historyDf['year'] = historyDf['timestamp'].dt.year
    historyDf['month'] = historyDf['timestamp'].dt.month
    historyDf['day'] = historyDf['timestamp'].dt.day
    historyDf['hour'] = historyDf['timestamp'].dt.hour
    historyDf['minute'] = historyDf['timestamp'].dt.minute
    historyDf['second'] = historyDf['timestamp'].dt.second

    # 添加年-月-日、小时:分钟的列
    historyDf['date'] = historyDf['timestamp'].dt.date
    historyDf['time'] = historyDf['timestamp'].dt.time

    # 根据日期判断是否为工作日
    historyDf['is_workday_or_holiday'] = historyDf['date'].apply(lambda date: 0 if is_workday(date) else 1)

    historyDf['timeByMin'] = historyDf['hour'] * 60 + historyDf['minute'] + historyDf['second'] / 60

    if needNative:
        # 在每个用户进行某项行为的时间，为该用户创建一个新的列，标记为正样本
        historyDf['action_taken'] = 1

    if full is False:
        # 删除 timestamp、year、month、day、hour、minute、second、date、time 列
        historyDf.drop(['year', 'month', 'day', 'hour', 'minute', 'second', 'date', 'time'], axis=1,
                       inplace=True)

    # 获取所有的用户
    all_users = historyDf['user_id'].unique()
    if full:
        # 使用文件，避免每次都要重新计算
        if os.path.exists('./data/模型数据/user_history_' + str(needUserInfo) + "_" + str(needNative) + "_" + str(
                full) + '.json'):
            user_history = load_user_history(
                filename='./data/模型数据/user_history_' + str(needUserInfo) + "_" + str(needNative) + "_" + str(
                    full) + '.json')
            print('user_history.json loaded')
        else:
            user_history = get_user_history_parallel(historyDf)
            save_user_history(user_history, filename='./data/模型数据/user_history_' + str(needUserInfo) + "_" + str(
                needNative) + "_" + str(full) + '.json')
            print('user_history.json saved')

    # negative_samples = []
    # pbar = tqdm(historyDf.iterrows(), total=len(historyDf), position=1)
    #
    # for _, action in pbar:
    #     user = action['user_id']
    #     year, month, day = action['timestamp'].year, action['timestamp'].month, action['timestamp'].day
    #     num_positive_samples =1
    #     hisTime = user_history[user]
    #
    #     random_timestamp1 = generate_random_timestamps(num_positive_samples, year, month, day)
    #     random_timestamp1 = random_timestamp1[~random_timestamp1.isin(hisTime)]
    #
    #     random_timestamp2 = generate_random_timestamps(num_positive_samples, year)
    #     random_timestamp2 = random_timestamp2[~random_timestamp2.isin(hisTime)]
    #
    #     time1 = 1
    #     while len(random_timestamp1) < num_positive_samples:
    #         if time1 >= 6:
    #             break
    #         additional_samples_needed = num_positive_samples - len(random_timestamp1)
    #         new_random_timestamps = generate_random_timestamps(additional_samples_needed*time1, year, month, day)
    #         new_random_timestamps = random_timestamp1[
    #             ~random_timestamp1.isin(historyDf.loc[historyDf['user_id'] == user, 'timestamp'])]
    #         new_random_timestamps = new_random_timestamps[:num_positive_samples]
    #         random_timestamp1 = random_timestamp1.append(new_random_timestamps)
    #         time1 += 1
    #
    #     time2 = 1
    #     while len(random_timestamp2) < num_positive_samples:
    #         if time2 >= 6:
    #             break
    #         additional_samples_needed = num_positive_samples - len(random_timestamp2)
    #         new_random_timestamps = generate_random_timestamps(additional_samples_needed*time2, year)
    #         new_random_timestamps = random_timestamp2[
    #             ~random_timestamp2.isin(historyDf.loc[historyDf['user_id'] == user, 'timestamp'])]
    #         new_random_timestamps = new_random_timestamps[:num_positive_samples]
    #         random_timestamp2 = random_timestamp2.append(new_random_timestamps)
    #         time2 += 1
    #
    #     if len(random_timestamp1) != 0:
    #         negative_samples.append({
    #             'user_id': user,
    #             'timestamp': random_timestamp1[0],
    #             'is_workday_or_holiday': 0 if is_workday(random_timestamp1[0]) else 1
    #         })
    #     if len(random_timestamp2) != 0:
    #         negative_samples.append({
    #             'user_id': user,
    #             'timestamp': random_timestamp2[0],
    #             'is_workday_or_holiday': 0 if is_workday(random_timestamp2[0]) else 1
    #         })
    #
    # negative_samples = pd.DataFrame(negative_samples)
    if needNative:
        # 负样本的生成
        # 定义一个函数，生成每个用户的负样本

        # 等比划分用户列表
        threadingNum = getThreading(hard=1.5, mem=0.13)

        num_chunks = int(threadingNum * threadingNum / 2)
        his_chunks = np.array_split(historyDf, num_chunks)

        # 使用joblib并行生成负样本
        negative_samples_list = Parallel(n_jobs=threadingNum, verbose=50)(
            delayed(generate_negative_samples_for_users)(i, his_chunks[i]) for i in range(len(his_chunks)))

        # 将所有的负样本合并
        negative_samples = pd.concat(negative_samples_list)

        # 导出为csv
        negative_samples.to_csv('./data/模型数据/negative_samples.csv', index=False)

        # 为负样本添加其他特征，如年、月、日、小时、分钟等
        negative_samples['year'] = negative_samples['timestamp'].dt.year
        negative_samples['month'] = negative_samples['timestamp'].dt.month
        negative_samples['day'] = negative_samples['timestamp'].dt.day
        negative_samples['hour'] = negative_samples['timestamp'].dt.hour
        negative_samples['minute'] = negative_samples['timestamp'].dt.minute
        negative_samples['second'] = negative_samples['timestamp'].dt.second
        negative_samples['date'] = negative_samples['timestamp'].dt.date
        negative_samples['time'] = negative_samples['timestamp'].dt.time
        negative_samples['action_taken'] = 0

        # 拼接正负样本
        historyDf = pd.concat([historyDf, negative_samples])

    # 按照timestamp进行排序
    historyDf = historyDf.sort_values(by='user_id')

    if needUserInfo:
        # 用户信息
        with open('./data/user_info.json', 'rb') as f:
            data1 = orjson.loads(f.read())

        userInfoDf = pd.DataFrame(data1)

        # 将用户信息与历史行为数据合并
        df = pd.merge(historyDf, userInfoDf, left_on='user_id', right_on='user_id')
    else:
        df = historyDf
    # 输出数据量
    print("数据量：", df.shape)

    # 导出为csv
    df.to_csv('./data/模型数据/user_history_' + str(needUserInfo) + "_" + str(needNative) + "_" + str(full) + '.json',
              index=False)

    return df
