import pandas as pd
from joblib import Parallel, delayed
import orjson as json
import numpy as np
from tqdm import tqdm


def calculate_metrics_per_batch(i, predictions_batch, test_data):
    results = []
    for prediction in tqdm(predictions_batch, desc=f"batch {i}", position=1, leave=True):
        triple_id = prediction["triple_id"]
        candidate_voter_list = prediction["candidate_voter_list"]

        # 获取真实的voter
        for x in test_data:
            if x['triple_id'] == triple_id:
                true_voter = x['voter_id']
                inviter_id = x['inviter_id']
                item_id = x['item_id']
                timestamp = x['timestamp']
                break
        if true_voter in candidate_voter_list:
            rank = candidate_voter_list.index(true_voter) + 1
            mrr = 1.0 / rank
            hit_at_5 = 1 if rank <= 5 else 0
        else:
            mrr = 0.0
            hit_at_5 = 0

        results.append({
            "triple_id": triple_id,
            'inviter_id': inviter_id,
            'item_id': item_id,
            'timestamp': timestamp,
            "predicted_voters": candidate_voter_list,
            "predicted_len": len(candidate_voter_list),
            "true_voter": true_voter,
            "mrr": mrr,
            "hits_at_5": hit_at_5
        })
    return results


def calculate_metrics(test_data, predictions):
    # 将预测数据集划分为10份
    predictions_batches = np.array_split(predictions, 12)

    # 对每一份预测数据集进行并行处理
    results_batches = Parallel(n_jobs=12)(
        delayed(calculate_metrics_per_batch)(i, predictions_batches[i], test_data) for i in
        range(len(predictions_batches)))

    # 将结果合并
    results = [item for sublist in results_batches for item in sublist]

    # 计算总的MRR和HITS@5
    total_mrr = sum(result['mrr'] for result in results) / len(predictions)
    total_hits_at_5 = sum(result['hits_at_5'] for result in results) / len(predictions)

    return total_mrr, total_hits_at_5, results


# 读取测试数据和预测结果
with open('./data/item_share_train_info_test_01_ans.json', 'rb') as f:
    test_data = json.loads(f.read())

with open('./bin/x64/Release/submit.json', 'rb') as f:
    predictions = json.loads(f.read())

# 计算指标
total_mrr, total_hits_at_5, results = calculate_metrics(test_data, predictions)

print(f"总的MRR: {total_mrr}")
print(f"总的HITS@5: {total_hits_at_5}")

# 保存每一个query的结果到excel文件中
df = pd.DataFrame(results)
df.to_excel("./results.xlsx", index=False)
