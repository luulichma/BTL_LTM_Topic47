# Báo cáo tham khảo: Game xếp đơn hàng (Nhóm BTL 06) — bản trích văn bản

> Trích tự động từ `BaoCao_GameXepDonHang_Nhom06.docx` (chỉ phần chữ, không gồm hình/sơ đồ). Xem file .docx gốc để có đầy đủ hình.


## I. Mở đầu


### 1.  Giới thiệu ứng dụng và phân tích yêu cầu


#### Giới thiệu ứng dụng

Game xếp đơn hàng là một trò chơi rèn luyện khả năng nhanh nhạy, quan sát và một chút trí nhớ. Trong trò chơi, 2 người chơi sẽ đóng vai là 2 nhân viên giao đồ của một siêu thị thi đấu với nhau xem ai là người hoàn thành tốt hơn các yêu cầu đơn hàng.  
Người chơi sẽ quan sát kỹ các đồ vật trong khung yêu cầu của khách hàng, sau đó di chuyển trong siêu thị và nhặt các đồ vật đó đưa về cho khách theo đúng thứ tự yêu cầu. Đây là trò chơi online đối kháng giữa 2 người.  
Dựa vào kiến thức của môn Lập trình mạng, nhóm 6 đã triển khai thành công hệ thống game theo mô tả trên. Nhóm đã xây dựng hệ thống client-server với một server và nhiều client, cho phép người chơi gửi yêu cầu chơi game lên server và tham gia thi đấu đối kháng trực tiếp với nhau. Sau khi hoàn thành, người chơi có thể kết bạn, thách đấu nhau và lịch sử các trận đấu.  

#### Phân tích yêu cầu

Hệ thống: Hệ thống bao gồm một server và nhiều client kết nối với nhau. Server sẽ quản lý các trận đấu và lưu trữ thông tin người chơi.  
Đăng nhập: Người chơi cần đăng nhập vào tài khoản của mình từ một máy client. Sau khi đăng nhập thành công, giao diện hiển thị trang chủ gồm các chức năng:  
Ghép ngẫu nhiên  
Xem danh sách bạn bè  
Xem lịch sử đấu  
Thách đấu bạn bè  
Kết bạn  
Bắt đầu trận đấu:   
Có 2 cách để bắt đầu trận đấu  
Thách đấu bạn bè. Ấn nút thách đấu bạn bè trong danh sách bạn bè, một trận đấu giữa 2 người được bắt đầu.  
Tham gia ghép ngẫu nhiên. Khi tìm đủ 2 người ghép ngẫu nhiên thì một trận đấu được bắt đầu.  
Luật chơi: Trò chơi Game xếp đồ vật siêu thị diễn ra như sau:  
Sau khi ghép trận thành công, 2 người chơi được đưa vào cùng một phòng đấu, server sẽ gửi về 6 số nguyên ngẫu nhiên khác nhau (từ 0 đến 9) đại diện cho 6 item hiện có trong siêu thị (mỗi ván đấu sẽ có đồ vật khác nhau). Các client nhận các id đó và hiện các item của siêu thị lên màn chơi.  
Người chơi sử dụng các phím WASD và J K để điều khiển nhân vật hoàn thành các yêu cầu khách hàng tại quầy order của mình.   
WASD - di chuyển  
J - nhặt đồ / đưa đồ cho khách  
K - thả đồ (nếu nhặt sai)  
Người chơi cần di chuyển lại gần khách hàng đó để biết họ yêu cầu món đồ gì, khi rời xa vị khách đó, bong bóng yêu cầu sẽ ẩn đi.  
Khi hoàn thành yêu cầu của một khách hàng, người chơi nhận được 5 điểm.  
Kết thúc 2p30s, người chơi nào được nhiều điểm hơn sẽ giành chiến thắng.  
Giao diện: Giao diện trò chơi hiển thị hình ảnh 6 đồ vật của siêu thị, 2 người chơi là 2 nhân viên di chuyển xung quanh siêu thị, mỗi người chơi có một quầy khách hàng của riêng mình. Mỗi khách hàng tới đều yêu cầu 3 món hàng.  
Kết thúc trận đấu: Trò chơi diễn ra trong 2 phút 30s. Người chơi nào có nhiều điểm hơn sẽ dành chiến thắng.  
Thoát khỏi trận đấu: Người chơi có thể thoát khỏi trận đấu bất cứ lúc nào và người chơi còn lại sẽ dành chiến thắng.  

#### Phân chia giao diện 

Họ và tên  
Giao diện   
Nguyễn Thị Tú Anh  
Giao diện danh sách bạn bè  
Giao diện thông báo kết bạn  
Giao diện kết bạn  
Nguyễn Trung Kiên  
Giao diện đăng nhập  
Giao diện home  
Giao diện trận đấu  
Lê Đức Hiếu  
Giao diện trận đấu  
Giao diện kết quả trận đấu  
Ngô Đức Sơn  
Giao diện lịch sử đấu  
Giao diện thách đấu  

#### Phân chia chức năng

Họ và tên  
Chức năng  
Nguyễn Thị Tú Anh  
Gameplay chính:  
Quản lý số đồ vật trong siêu thị  
Quản lý bạn bè  
Nguyễn Trung Kiên  
Đăng nhập  
Gameplay chính:  
Di chuyển  
Nhặt đồ, trả đồ cho khách  
Ghép trận ngẫu nhiên  
Lê Đức Hiếu  
Gameplay chính:  
Xử lý điểm số  
Xử lý kết quả trận đấu  
Chức năng thách đấu bạn bè  
Ngô Đức Sơn  
Gameplay chính:  
Quản lý yêu cầu khách hàng  
Chức năng xem lịch sử đấu  

#### Phân tích các nội dung cá nhân

Với chức năng đăng nhập:  
Khi vừa vào giao diện đăng nhập, Client gửi kết nối TCP tới Server, Server coi Client là một ẩn danh. Khi client nhập tài khoản mật khẩu và ấn “Connect”, Client gửi một yêu cầu đăng nhập tới Server. Server sẽ xử lý thông tin đăng nhập, nếu trùng khớp thì trả về cho Client thông tin người dùng. Client nhận được thì load vào giao diện Home.  
Với chức năng ghép trận ngẫu nhiên:  
Ở giao diện màn hình chính, người chơi chọn vào nút “Play”.   
Yêu cầu tìm trận được gửi về phía Server, phía server sẽ đẩy client này vào hàng đợi ở phía server, khi có 2 người trong hàng đợi thì gửi id đối thủ và id phòng cho client. Client sẽ hiển thị giao diện vào game   
Với chức năng Di chuyển trong Gameplay:  
Trong màn gameplay, Client ấn nút WASD sẽ liên tục gửi vị trí và trạng thái hiện tại tới server bằng UDP, server nhận được thông tin và trả về cho đối thủ. Tại client của đối thủ, thông tin nhận được đó sẽ hiển thị vị trí của người chơi liên tục.  
Với chức năng nhặt đồ trả đồ cho khách:  
Trong màn gameplay, Client ấn J nếu có đồ vật nào ở gần thì sẽ gửi một gói tin tới server bao gồm id của đồ vật đó, Server cũng sẽ gửi tới đối thủ để client đó được nhìn thấy người chơi đang cầm vật gì. Client ấn J tại hàng đợi của khách hàng và đang cầm đồ vật, Client sẽ xử lý xem có trùng khớp với đồ vật mà khách hàng đó đang yêu cầu, gửi thông tin đúng sai đến server.   

## II. Phân tích thiết kế


### Phân tích thiết kế tổng quan ứng dụng


#### a. Kiến trúc tổng quan

Hình 1. Kiến trúc tổng quan  

#### b. Sơ đồ khối chức năng của Server, Client

Server  
Hình 2. Sơ đồ khối phía Server  
Client  
Hình 3. Sơ đồ khối phía Client  

#### c. Biểu đồ Usecase toàn hệ thống

Hình 4. Sơ đồ Use Case chi tiết toàn hệ thống  

### Phần cá nhân


#### Use case chi tiết

Hình 5. Sơ đồ Use Case chi tiết cá nhân  

#### Biểu đồ lớp

Hình 6. Biểu đồ lớp MVC phần cá nhân  

#### Biểu đồ tuần tự

Biểu đồ tuần tự Đăng nhập  
Hình 7. Biểu đồ tuần tự module Đăng nhập  
Biểu đồ tuần tự Tìm trận ngẫu nhiên  
Hình 8. Biểu đồ tuần tự Tìm trận ngẫu nhiên  
Biểu đồ tuần tự Gameplay - di chuyển  
Hình 9. Biểu đồ tuần tự module di chuyển  
Biểu đồ tuần tự Gameplay - tương tác đồ vật  
Hình 10. Biểu đồ tuần tự module tương tác đồ vật  

#### Sơ đồ thực thể quan hệ E-R

Hình 12. Sơ đồ thực thể quan hệ E-R  

## III. Kết quả


### Kiến trúc hệ thống


#### Tổng quan kiến trúc

Sử dụng mô hình Client - Server:  
Client (Người chơi): Chịu trách nhiệm gửi yêu cầu (request) và nhận phản hồi (response) từ server. Giao diện của client được xây dựng bằng Game Engine Unity  
Server (Máy chủ): Chịu trách nhiệm xử lý các logic của trò chơi bao gồm các công việc như kết nối các người chơi, tổ chức các trận đấu, xử lý xác thực người chơi, các chức năng liên quan đến phòng đấu, giao dịch trong trò chơi và truyền dữ liệu đến client.   
Giao thức sử dụng là TCP và UDP  

#### Quy trình hoạt động

Kết nối: Client kết nối đến server thông qua TCP  
Truyền dữ liệu: Client gửi server các gói tin bao gồm dữ liệu là các dữ liệu và loại sự kiện của dữ liệu đó. Server nhận được dữ liệu, xử lý các logic và gửi lại cho client các kết quả tương ứng. Client nhận được dữ liệu từ Server, xử lý logic và hiển thị lên giao diện cho người dùng.  

### Cài đặt và triển khai ứng dụng

Client (Unity)  
Engine: Unity 6000.2.7f2  
Ngôn ngữ: C#  
Render Pipeline: Universal Render Pipeline (URP) 17.2.0  
UI: Unity UI (Canvas, Text, Button) kết hợp TextMeshPro  
Thư viện: DOTween (xử lý animations)  
Server (C#)  
Runtime: .NET 9.0  
Ngôn ngữ: C#  
Cơ sở dữ liệu: MySQL (MySql.Data connector)  
Networking: System.Net.Sockets (TCP/UDP)  
Giao thức mạng:  
TCP: đăng nhập, matchmaking, hệ thống bạn bè, thách đấu, cập nhật điểm số  
UDP: đồng bộ chuyển động người chơi (player movement / position sync)  

### Các kết quả thực hiện được (phần giao diện trong module của cá nhân)

Giao diện đăng nhập  
Giao diện Home  
Giao diện kết bạn  
Giao diện lịch sử đấu  
Giao diện notification  
Giao diện thách đấu  
Giao diện gameplay  
Giao diện kết quả trận đấu  

## IV. Tài liệu tham khảo

1. Giáo trình môn Lập trình mạng - Nguyễn Mạnh Hùng, Nguyễn Trọng Khánh   
2. Giáo trình môn Lập trình mạng (2010) - Hà Mạnh Đào   
3. Hotel reservation management Application – Software: Art of Design and Programming  