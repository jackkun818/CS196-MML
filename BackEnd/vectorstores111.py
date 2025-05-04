import os
from langchain_community.vectorstores import Chroma
from langchain_community.embeddings import SentenceTransformerEmbeddings
from langchain_community.utilities import SQLDatabase

# embeddings = SentenceTransformerEmbeddings(model_name='all-mpnet-base-v2')
embeddings = SentenceTransformerEmbeddings(model_name="F:/194/all-mpnet-base-v2")
embeddings_new = SentenceTransformerEmbeddings(model_name="F:/194/m3e-base")

dir = "F:/194/"

db = SQLDatabase.from_uri(f"sqlite:///{dir}data/patient.db")
training_db = SQLDatabase.from_uri(f"sqlite:///{dir}data/training.db")

vectorstore_patient_evaluation = Chroma(persist_directory=os.path.join(dir,"data/vectorstore_patient_evaluation"), embedding_function=embeddings)
vectorstore_backgroundinfo = Chroma(persist_directory=os.path.join(dir,"data/vectorstore_backgroundinfo"), embedding_function=embeddings)
vectorstore_papers = Chroma(persist_directory=os.path.join(dir,"data/vectorstore_papers"), embedding_function=embeddings)

vectorstore_explain = Chroma(persist_directory=os.path.join(dir,"data/vectorstore_module_explain"), embedding_function=embeddings_new)
vectorstore_comfort = Chroma(persist_directory=os.path.join(dir,"data/vectorstore_comfort"), embedding_function=embeddings_new)
# vectorstore_guide = Chroma(persist_directory=os.path.join(dir,"data/vectorstore_module_explain"), embedding_function=embeddings_new)
vectorstore_guide = Chroma(persist_directory=os.path.join(dir,"data/vectorstore_guide_1012"), embedding_function=embeddings_new)