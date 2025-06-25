from pyproton.proton  import PyProtonClient

# Path to the ProtonDB CLI executable
exe_path = "D:\\STUDYYY\\ProtonDB\\ProtonDB.exe"


client = PyProtonClient(exe_path)


print("Creating database:")
print(client.create_db("school"))

print("Using database:")
print(client.use_db("vadachennai"))

print("Creating collection:")
print(client.create_collection("characters"))


print("Inserting documents:")
print(client.insert("characters", {"name": "Anbu", "age": 30}))
print(client.insert("characters", [{"name": "senthil"}, {"name": "guna"}]))

print("Printing documents:")
print(client.print_docs("characters"))

print("Printing filtered:")
print(client.print_docs("characters", condition="age>=18"))

print("Updating (add):")
print(client.update("characters", "add", {"kill_count": 100}))

print("Updating (drop):")
print(client.update("characters", "drop", {"score": None}))

print("Updating (alter):")
print(client.update("characters", "alter", {"name": "samuthirakanni"}, condition="name=guna"))

print("Collections:", client.list_collections())
print("Databases:", client.list_databases())


client.close()
