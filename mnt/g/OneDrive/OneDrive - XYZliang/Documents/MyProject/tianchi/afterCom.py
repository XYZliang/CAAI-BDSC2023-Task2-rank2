import pickle
import orjson
from tqdm import tqdm
from collections import defaultdict

from tools import statistics

with open("submit.json", "rb") as f:
    submitres = orjson.loads(f.read())

# 补足5人的继续推理：对于用户A的相似用户们，对于每一个相似用户，将A的相似用户们的相似用户补全到A中
print("推理完毕，开始对于推荐结果不足5人的，继续尝试递归补全：")
submitDict = {}
for obj in tqdm(submitres):
    triple_id = obj["triple_id"]
    candidate_voter_list = obj["candidate_voter_list"]
    # 将 "triple_id" 和 "candidate_voter_list" 组合成键值对，并添加到结果字典
    submitDict[triple_id] = candidate_voter_list
for res in tqdm(submitres):
    userID = res["triple_id"]
    voters = res["candidate_voter_list"]
    if len(voters) == 0:
        print(userID + "的voters为空，没救了")
        continue
    if len(voters) > 5:
        continue
    if len(voters) < 5:
        newVoters = voters
        for voter in voters:
            if voter not in submitDict:
                continue
            if len(submitDict[voter]) == 0:
                continue
            for voter2 in submitDict[voter]:
                if voter2 not in voters:
                    newVoters.append(voter2)
                    if len(newVoters) >= 5:
                        break
            if len(newVoters) >= 5:
                break
        if len(newVoters) != len(voters):
            print(userID + "补全后人数从" + str(len(voters)) + "变为" + str(len(newVoters)))
        if len(newVoters) < 5:
            print(userID + "补全后依旧不足五人")
        res["candidate_voter_list"] = newVoters[:5]

statistics(submitres, "张亮方案三递归推理0.3276",show=True)
# 将 submitres 列表转换为 JSON 格式的字符串 text
text = orjson.dumps(submitres)
# 将字符串 text 写入文件 submit.json
with open("submitCompleteStep2.json", "wb") as f:  # 注意这里是"wb"，因为orjson.dumps返回的是bytes而不是str
    f.write(text)
