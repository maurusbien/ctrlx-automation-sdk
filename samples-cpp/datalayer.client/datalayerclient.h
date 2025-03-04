#include <stdio.h>
#include <iostream>


#include "comm/datalayer/datalayer.h"
#include "comm/datalayer/datalayer_system.h"
#include "comm/datalayer/metadata_generated.h"

#include "sampleSchema_generated.h"

class DataLayerClient
{
private:
  std::string _ip;
  std::string _user;
  std::string _password;
  int _sslPort;
  comm::datalayer::IClient *_client;
  comm::datalayer::DlResult _result;
  comm::datalayer::DlResult _resultAsync;
  comm::datalayer::Variant _data;
  comm::datalayer::Variant _dataAsync;

  comm::datalayer::DatalayerSystem _datalayerSystem;

  comm::datalayer::IClient::ResponseCallback responseCallback();

  void println(const std::string &text, comm::datalayer::DlResult result, comm::datalayer::Variant *data);
  comm::datalayer::DlResult print(comm::datalayer::Variant *data);

  bool waitForResponseCallback(int counter);
  void readSync(const std::string node);
  void writeSync(const std::string node);
  void createSync(const std::string node);

  bool start();
  void ping();
  void read();

  void create();

  void remove();

  void browse();

  void write();

  void metadata();
  void stop();

public:
  DataLayerClient(const std::string &ip = "192.168.1.1", const std::string &user = "boschrexroth", const std::string &password = "boschrexroth", int sslPort = 443);
  void Run();
};
