import orjson as json

with open('./output/result/submitComplete.json', 'rb') as f:
    predictions = json.loads(f.read())
for item in predictions:
    tid = item['triple_id']
    Plist = item['candidate_voter_list']
    if len(Plist) > 5:
        print(tid, len(Plist))
        Plist = Plist[:5]
