# This R script is based on Sandro's python script, which produces a LB score of 0.8655
# This script should produce a LB score of 0.86547

# load libraries
library(xgboost)
library(readr)
library(stringr)
library(caret)
library(car)

set.seed(654321)

# load data
df_train = read_csv("~/Documents/Kaggle/AirBnB/train_users_2.csv")
df_test = read_csv("~/Documents/Kaggle/AirBnB/test_users.csv")
labels = df_train['country_destination']
df_train = df_train[-grep('country_destination', colnames(df_train))]

# combine train and test data
df_all = rbind(df_train,df_test)
# remove date_first_booking
df_all = df_all[-c(which(colnames(df_all) %in% c('date_first_booking')))]
# replace missing values
# df_all$timestamp_first_active[df_all$timestamp_first_active == ""] = "2020-12-31"
df_all[is.na(df_all)] <- -1

# date feature engineering
df_all$date_account_created1 = as.Date(df_all$date_account_created)
df_all$timestamp_first_active1=as.Date(as.character(df_all$timestamp_first_active), "%Y%m%d%H%M%S")

df_all['diff1'] = df_all['date_account_created1'] - df_all['timestamp_first_active1']
df_all$dac_wday = as.numeric(as.POSIXlt(df_all$date_account_created1)$wday)
df_all$dac_year = as.numeric(as.POSIXlt(df_all$date_account_created1)$year+1900)
df_all$dac_month = as.numeric(as.POSIXlt(df_all$date_account_created1)$mon+1)
df_all$dac_day = as.numeric(as.POSIXlt(df_all$date_account_created1)$mday)

df_all$tfa_wday = as.numeric(as.POSIXlt(df_all$timestamp_first_active1)$wday)
df_all$tfa_year = as.numeric(as.POSIXlt(df_all$timestamp_first_active1)$year+1900)
df_all$tfa_month = as.numeric(as.POSIXlt(df_all$timestamp_first_active1)$mon+1)
df_all$tfa_day = as.numeric(as.POSIXlt(df_all$timestamp_first_active1)$mday)

df_all = df_all[,-c(which(colnames(df_all) %in% c('date_account_created1')))]
df_all = df_all[,-c(which(colnames(df_all) %in% c('timestamp_first_active1')))]
df_all = df_all[,-c(which(colnames(df_all) %in% c('date_account_created')))]
df_all = df_all[,-c(which(colnames(df_all) %in% c('timestamp_first_active')))]

# clean Age by removing values
df_all[df_all$age < 14 | df_all$age > 100,'age'] <- -1

# one-hot-encoding features
ohe_feats = c('gender', 'signup_method', 'language', 'affiliate_channel', 'affiliate_provider', 'first_affiliate_tracked', 'signup_app', 'first_device_type', 'first_browser')
dummies <- dummyVars(~ gender + signup_method + language + affiliate_channel + affiliate_provider + first_affiliate_tracked + signup_app + first_device_type + first_browser, data = df_all)
df_all_ohe <- as.data.frame(predict(dummies, newdata = df_all))
df_all_combined <- cbind(df_all[,-c(which(colnames(df_all) %in% ohe_feats))],df_all_ohe)

# split train and test
X = df_all_combined[df_all_combined$id %in% df_train$id,]
y <- recode(labels$country_destination,"'NDF'=0; 'US'=1; 'other'=2; 'FR'=3; 'CA'=4; 'GB'=5; 'ES'=6; 'IT'=7; 'PT'=8; 'NL'=9; 'DE'=10; 'AU'=11")
X_test = df_all_combined[df_all_combined$id %in% df_test$id,]

# train xgboost
xgb <- xgboost(data = data.matrix(X[,-1]), 
               label = y, 
               eta = 0.1,
               max_depth = 9, 
               nround=50, 
               subsample = 0.5,
               colsample_bytree = 0.5,
               seed = 1,
               eval_metric = "merror",
               objective = "multi:softprob",
               num_class = 12,
               nthread = 3
)
#feature importance
names <- dimnames(data.matrix(X[,-1]))[[2]]
importance_matrix <- xgb.importance(names, model = xgb)
xgb.plot.importance(importance_matrix[1:80,])
xgb.plot.tree(feature_names = names, model = xgb, n_first_tree = 2)

# predict values in test set
y_pred <- predict(xgb, data.matrix(X_test[,-1]))

# extract the 5 classes with highest probabilities
predictions <- as.data.frame(matrix(y_pred, nrow=12))
rownames(predictions) <- c('NDF','US','other','FR','CA','GB','ES','IT','PT','NL','DE','AU')
predictions_top5 <- as.vector(apply(predictions, 2, function(x) names(sort(x)[12:8])))

# create submission 
ids <- NULL
for (i in 1:NROW(X_test)) {
  idx <- X_test$id[i]
  ids <- append(ids, rep(idx,5))
}
submission <- NULL
submission$id <- ids
submission$country <- predictions_top5

# generate submission file
submission <- as.data.frame(submission)
write.csv(submission, "submission.csv", quote=FALSE, row.names = FALSE)
