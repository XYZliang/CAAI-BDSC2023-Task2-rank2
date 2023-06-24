import json
import pandas as pd
from gensim import corpora, models
from gensim.models import LdaMulticore
from joblib import Parallel, delayed
from sklearn.cluster import KMeans
from tqdm import tqdm
import logging

logging.basicConfig(format='%(asctime)s : %(levelname)s : %(message)s', level=logging.INFO)


def train_lda_model(num_topics,corpus,dictionary):

    print("Training LDA model...")
    lda = LdaMulticore(corpus=corpus, num_topics=num_topics, id2word=dictionary, passes=15, workers=8)

    print("Getting topic distributions...")
    topics = [lda[c] for c in tqdm(corpus)]

    # print("Processing topic distributions...")
    # dense_topics = []
    # for topic in tqdm(topics):
    #     dense_topic = [0] * num_topics
    #     for i, prob in topic:
    #         dense_topic[i] = prob
    #     dense_topics.append(dense_topic)

    print('\nPerplexity: ', lda.log_perplexity(corpus))
    return lda.log_perplexity(corpus)
    #
    # print("Performing K-Means clustering...")
    # kmeans = KMeans(n_clusters=3).fit(dense_topics)
    #
    # print("Clustering results:")
    # print(kmeans.labels_)
    # print(len(kmeans.labels_))

if __name__ == '__main__':
    with open('./data/user_info.json', 'r') as f:
        dat1a = json.load(f)
    print(len(dat1a))

    with open('./data/item_share_train_info_AB.json', 'r') as f:
        data = json.load(f)

    df = pd.DataFrame(data)

    print("Processing voter data...")
    voter_items = df.groupby('voter_id')['item_id'].apply(list).reset_index()
    voter_items.columns = ['user_id', 'items']

    print("Processing inviter data...")
    inviter_items = df.groupby('inviter_id')['item_id'].apply(list).reset_index()
    inviter_items.columns = ['user_id', 'items']

    print("Merging data...")
    all_items = pd.concat([voter_items, inviter_items]).groupby('user_id')['items'].sum().reset_index()
    num_users = all_items.shape[0]
    print(f'The number of unique users is {num_users}')

    print("Creating dictionary...")
    dictionary = corpora.Dictionary(all_items['items'])

    print("Creating corpus...")
    corpus = [dictionary.doc2bow(text) for text in tqdm(all_items['items'])]
    # 调用函数并传入不同的主题数
    # 调用函数并传入不同的主题数
    num_topics_list = [10,20,40,100]  # 可根据需要修改主题数的范围
    corpuslist = Parallel(n_jobs=2)(
        delayed(train_lda_model)(num_topics, corpus, dictionary) for num_topics in tqdm(num_topics_list, position=1)
    )

    print(corpuslist)