
# coding: utf-8

# In[8]:

bad_words = ['*	0	+']

with open('test.txt') as oldfile, open('newfile.txt', 'w') as newfile:
    for line in oldfile:
        if not any(bad_word in line for bad_word in bad_words):
            newfile.write(line)


# In[ ]:



