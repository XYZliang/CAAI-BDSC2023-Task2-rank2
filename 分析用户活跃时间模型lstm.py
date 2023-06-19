import pandas as pd  # 导入pandas库，用于数据处理
import tensorflow as tf  # 导入Tensorflow库，用于建立和训练深度学习模型
import orjson  # 导入orjson库，用于处理JSON格式的数据
from tensorflow.keras.models import Sequential  # 从Tensorflow库中导入Sequential模型，这是一个顺序模型
from tensorflow.keras.layers import LSTM, Dense, Embedding  # 从Tensorflow库中导入LSTM，Dense和Embedding层
from tensorflow.keras.optimizers import Adam  # 从Tensorflow库中导入Adam优化器
from sklearn.model_selection import train_test_split  # 从sklearn库中导入train_test_split，用于切分训练集和测试集
from sklearn.preprocessing import LabelEncoder  # 从sklearn库中导入LabelEncoder，用于标签编码
from tensorflow.keras.layers import Input, concatenate  # 从Tensorflow库中导入Input和concatenate，用于模型输入和拼接层
from tensorflow.keras.models import Model  # 从Tensorflow库中导入Model，用于建立模型
from tensorflow.keras.layers import Flatten, Reshape  # 从Tensorflow库中导入Flatten和Reshape，用于改变Tensor的形状
import pickle  # 导入pickle库，用于Python对象的序列化和反序列化
from tensorflow.keras.callbacks import EarlyStopping  # 从Tensorflow库中导入EarlyStopping，用于提前停止训练
import datetime  # 导入datetime库，用于处理时间和日期
import numpy as np  # 导入numpy库，用于科学计算
from sklearn.preprocessing import OneHotEncoder  # 从sklearn库中导入OneHotEncoder，用于进行独热编码
from sklearn.impute import SimpleImputer  # 从sklearn库中导入SimpleImputer，用于填充缺失值
from sklearn.preprocessing import MinMaxScaler  # 从sklearn库中导入MinMaxScaler，用于进行归一化操作

# 读取CSV文件，将其转换为pandas DataFrame对象
df = pd.read_csv('./data/模型数据/df.csv')

# 使用LabelEncoder对'user_id'列进行编码，并将结果存储在'user_id_fit'列中
le = LabelEncoder()
df['user_id_fit'] = le.fit_transform(df['user_id'])

# 将编码器保存到pickle文件中，以便后续使用
with open('./data/模型数据/label_encoder.pkl', 'wb') as f:
    pickle.dump(le, f)

# 删除 'minute', 'second', 'date', 'time'列
df.drop(columns=['minute', 'second', 'date', 'time'], inplace=True)

# 将'month', 'day', 'hour'列的值映射到[-1, 1]范围内，用于处理周期性特征
df['month_sin'] = np.sin(2 * np.pi * df['month'] / 12)
df['month_cos'] = np.cos(2 * np.pi * df['month'] / 12)
df['day_sin'] = np.sin(2 * np.pi * df['day'] / 31)
df['day_cos'] = np.cos(2 * np.pi * df['day'] / 31)
df['hour_sin'] = np.sin(2 * np.pi * df['hour'] / 24)
df['hour_cos'] = np.cos(2 * np.pi * df['hour'] / 24)

# 对'user_gender'列进行独热编码
encoder = OneHotEncoder(sparse=False)
df['user_gender'] = encoder.fit_transform(df['user_gender'].values.reshape(-1, 1))

# 对'user_age'和'user_level'列进行归一化处理
scaler = MinMaxScaler()
df[['user_age', 'user_level']] = scaler.fit_transform(df[['user_age', 'user_level']])

# 使用SimpleImputer对'user_gender', 'user_age', 'user_level'列的缺失值进行填充
imputer = SimpleImputer(strategy='mean')
df[['user_gender', 'user_age', 'user_level']] = imputer.fit_transform(df[['user_gender', 'user_age', 'user_level']])

# 更新用于模型训练的特征列表
features = ['year', 'month_sin', 'month_cos', 'day_sin', 'day_cos', 'hour_sin', 'hour_cos',
            'is_workday_or_holiday', 'user_age', 'user_gender', 'user_level']

# 切分训练集和测试集
train_df, test_df = train_test_split(df, test_size=0.2, random_state=42)

# 将输入数据转换为模型所需的格式
train_features = train_df[features].values.reshape((-1, len(features), 1))
test_features = test_df[features].values.reshape((-1, len(features), 1))

# 定义模型的输入和嵌入层
input_user_id = Input(shape=(1,))
embedding_layer = Embedding(input_dim=len(le.classes_), output_dim=50, input_length=1)(input_user_id)
embedding_layer = Flatten()(embedding_layer)
embedding_layer = Reshape((1, 50))(embedding_layer)

# 导入正则化库
from keras import regularizers

# 定义模型的LSTM层
input_other_features = Input(shape=(len(features), 1))
lstm_layer = LSTM(50, activation='tanh', return_sequences=False, dropout=0.1, recurrent_dropout=0.1,
                  kernel_regularizer=regularizers.l2(0.01), recurrent_regularizer=regularizers.l2(0.01))(
    input_other_features)
lstm_layer = Reshape((1, 50))(lstm_layer)

# 将嵌入层和LSTM层的输出进行拼接
concat_layer = concatenate([embedding_layer, lstm_layer], axis=1)

# 定义早停参数
early_stop = EarlyStopping(monitor='val_loss', patience=5)

# 定义模型的输出层
output = Dense(1, activation='sigmoid')(concat_layer)

# 定义模型，并设置损失函数和优化器
model = Model(inputs=[input_user_id, input_other_features], outputs=output)
model.compile(loss='binary_crossentropy', optimizer=Adam(learning_rate=0.0001), metrics=['accuracy'])

# 输出当前时间
print('在' + str(datetime.datetime.now()) + '开始训练模型')

# 训练模型
model.fit([train_df['user_id_fit'].values.reshape((-1, 1)),
           train_features],
          train_df['action_taken'],
          epochs=50,
          batch_size=8192,
          validation_data=([test_df['user_id_fit'].values.reshape((-1, 1)),
                            test_features],
                           test_df['action_taken']),
          verbose=True,
          shuffle=False,
          callbacks=[early_stop])
