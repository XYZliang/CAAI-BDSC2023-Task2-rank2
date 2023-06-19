import orjson as json
from tqdm import tqdm

# orsosn读取./bin/x64/Release/conf.json
with open('./bin/x64/Release/conf.json', 'rb') as f:
    conf = json.loads(f.read())
nullCount = 0
nullList = []
T3List = []
T3Num = 0
for Qid, userList in tqdm(conf.items()):
    for user, scoreLists in userList.items():
        for scoreType, scoreList in scoreLists.items():
            if scoreType == "null" or scoreType is None:
                nullCount += 1
                nullList.append(Qid)
                continue
            if len(scoreList) > 1:
                print(Qid + "用户有多个分数")
            if scoreType == "scoreT3" and len(scoreLists) < 5:
                # print(Qid+"预测到第三层还不足")T3Num += 1
                # 如果scoreType是scoreLists.items()的最后一个元素，那么就是预测到第三层还不足
                if scoreType == list(scoreLists.items())[-1][0]:
                    T3Num += len(scoreList)
                    T3List.append(Qid)

T3List = list(set(T3List))
nullList = list(set(nullList))
print("总共"+str(len(conf))+"个问题")
print("未知分数有" + str(len(nullList)) + "个问题，有" + str(nullCount) + "数据")
print("预测到第三层还不足有" + str(len(T3List)) + "个问题，占比"+str(len(T3List)/len(conf))+"，平均每组"+str(T3Num/len(T3List))+"个数据")

