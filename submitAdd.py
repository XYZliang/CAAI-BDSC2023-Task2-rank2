from tools import  *


if __name__ == "__main__":
    # orjson读取G:\OneDrive\OneDrive - XYZliang\Documents\MyProject\tianchi\bin\x64\Debug\submit.json
    with open("./output/mresult/submitComplete.json", "rb") as f:
        datas = orjson.loads(f.read())
    statistics(datas,"原始方案",True)

    # orjson读取"G:\OneDrive\OneDrive - XYZliang\Documents\MyProject\tianchi\0-2511-submission_A_1_0_5.json"
    with open("./0-2511-submission_A_1_0_5.json", "rb") as f:
        datas1 = orjson.loads(f.read())
    statistics(datas1,"方案1",True)

    datas = sorted(datas, key=lambda x: int(x['triple_id']))
    datas1 = sorted(datas1, key=lambda x: int(x['triple_id']))
    if len(datas)!=len(datas1):
        print("长度不一致")
        exit(0)

    for i in tqdm(range(len(datas))):
        data = datas[i]
        userID = data["triple_id"]
        voters = data["candidate_voter_list"]
        if len(voters) < 5:
            data["candidate_voter_list"].extend(datas1[i]["candidate_voter_list"])
            # 取前五
            data["candidate_voter_list"] = data["candidate_voter_list"][:5]
        if len(voters) > 5:
            data["candidate_voter_list"] = data["candidate_voter_list"][:5]

    # orjson写入
    with open("./submitplus.json", "wb") as f:
        f.write(orjson.dumps(datas))

    statistics(datas,"方案1+原始方案",True)