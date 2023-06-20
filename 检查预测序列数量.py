import orjson as json

with open('./bin/x64/Release/submit.json', 'rb') as f:
    predictions = json.loads(f.read())
for item in predictions:
    tid = item['triple_id']
    Plist = item['candidate_voter_list']
    if len(Plist) > 5:
        print(tid, len(Plist))
        Plist = Plist[:5]
# 保存
with open('./bin/x64/Release/submit1.json', 'wb') as f:
    f.write(json.dumps(predictions))
