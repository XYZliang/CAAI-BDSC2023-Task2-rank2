import numpy as np
import pandas as pd
import copy
from tqdm import tqdm
# 从训练集中划分出验证集
def splitValidationFromTraining(data,percentage):
    data = pd.DataFrame(data)
    #设置随机数种子，保证每次生成的结果都是一样的
    np.random.seed(2023)
    #permutation随机生成0-len(data)随机序列
    shuffled_indices = np.random.permutation(len(data))
    #test_ratio为测试集所占的半分比
    test_set_size = int(len(data) * percentage)
    test_indices = shuffled_indices[:test_set_size]
    train_indices = shuffled_indices[test_set_size:]
    #iloc选择参数序列中所对应的行
    return data.iloc[train_indices].to_dict(orient='records'), data.iloc[test_indices].to_dict(orient='records')

# 统计效果
def statistics(datas,desc,show=False):
    list = {
        "0":[],
        "1":[],
        "2":[],
        "3":[],
        "4":[],
        "5":[],
    }
    for data in datas:
        userID = data["triple_id"]
        voters = data["candidate_voter_list"]
        list[str(len(voters))].append(userID)
    resultList = {
        "方案":desc,
        "0长度数组":list["0"],
        "1长度数组":list["1"],
        "2长度数组":list["2"],
        "3长度数组":list["3"],
        "4长度数组":list["4"],
        "5长度数组":list["5"],
        "0长度数组数量":len(list["0"]),
        "1长度数组数量":len(list["1"]),
        "2长度数组数量":len(list["2"]),
        "3长度数组数量":len(list["3"]),
        "4长度数组数量":len(list["4"]),
        "5长度数组数量":len(list["5"]),
    }
    if show:
        print(resultList)
    return resultList

def makeStatistics(datas,savePath):
    # 转为df
    df = pd.DataFrame(datas)
    # 转为excel
    df.to_excel(savePath+"statisticsResult.xlsx")

def del_inviter_id(data):
    datas = copy.deepcopy(data)
    for obj in datas:
        del obj["inviter_id"]
    return datas

def getValueFrom2Dict(dict,key1,key2,default=-1):
    try:
        return dict[key1][key2]
    except KeyError:
        return default