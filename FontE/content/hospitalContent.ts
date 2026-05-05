export type QuickAction = {
  title: string;
  description: string;
  href: string;
  accent: string;
};

export type ServiceCategory = {
  title: string;
  summary: string;
  items: string[];
};

export type Specialty = {
  title: string;
  lead: string;
  description: string;
};

export type DoctorProfile = {
  name: string;
  specialty: string;
  title: string;
  experience: string;
  focus: string[];
};

export type NewsArticle = {
  title: string;
  category: string;
  summary: string;
  readTime: string;
};

export const hospitalStats = [
  { value: "24/7", label: "Tổng đài và hỗ trợ đặt lịch" },
  { value: "120+", label: "Bác sĩ và chuyên gia thường trực" },
  { value: "18", label: "Chuyên khoa và đơn vị cận lâm sàng" },
  { value: "45'", label: "Cam kết phản hồi yêu cầu ưu tiên" },
];

export const quickActions: QuickAction[] = [
  {
    title: "Đặt lịch khám nhanh",
    description: "Đặt lịch theo chuyên khoa, bác sĩ hoặc khung giờ phù hợp cho gia đình.",
    href: "/booking",
    accent: "from-cyan-500 to-sky-700",
  },
  {
    title: "Lấy mẫu xét nghiệm tại nhà",
    description: "Điều phối điều dưỡng tới tận nơi, đồng bộ kết quả về hồ sơ bệnh án điện tử.",
    href: "/services",
    accent: "from-emerald-500 to-teal-700",
  },
  {
    title: "Tra cứu gói dịch vụ",
    description: "Xem lộ trình khám tổng quát, tầm soát chuyên sâu và doanh nghiệp.",
    href: "/services",
    accent: "from-blue-500 to-indigo-700",
  },
  {
    title: "Kết nối bác sĩ chuyên gia",
    description: "Tư vấn trước khám, theo dõi sau điều trị và nhắc lịch tái khám tự động.",
    href: "/doctors",
    accent: "from-slate-700 to-slate-900",
  },
];

export const serviceCategories: ServiceCategory[] = [
  {
    title: "Khám đa khoa cao cấp",
    summary: "Hệ thống tiếp đón riêng, quy trình tinh gọn và hồ sơ điều trị xuyên suốt.",
    items: ["Khám nội tổng quát", "Khám chuyên khoa theo lịch hẹn", "Theo dõi điều trị dài hạn"],
  },
  {
    title: "Xét nghiệm và chẩn đoán",
    summary: "Liên thông chỉ định, lấy mẫu, trả kết quả và đối chiếu xu hướng ngay trên hồ sơ.",
    items: ["Xét nghiệm sinh hóa - miễn dịch", "Chẩn đoán hình ảnh", "Tầm soát chuyên sâu"],
  },
  {
    title: "Sản phụ khoa và nhi",
    summary: "Mô hình chăm sóc gia đình với lịch sử khám đồng bộ cho mẹ và bé.",
    items: ["Theo dõi thai kỳ", "Khám phụ khoa định kỳ", "Nhi tổng quát và dinh dưỡng"],
  },
  {
    title: "Gói sức khỏe doanh nghiệp",
    summary: "Thiết kế chương trình theo ngành nghề, ngân sách và yêu cầu pháp lý.",
    items: ["Khám sức khỏe định kỳ", "Tầm soát theo rủi ro nghề nghiệp", "Báo cáo tổng hợp cho HR"],
  },
];

export const specialties: Specialty[] = [
  {
    title: "Tim mạch",
    lead: "Chăm sóc nguy cơ tim mạch theo mô hình dự phòng - chẩn đoán - theo dõi.",
    description: "Từ khám tầm soát, siêu âm tim, Holter huyết áp đến quản lý tăng huyết áp và bệnh mạch vành.",
  },
  {
    title: "Tiêu hóa",
    lead: "Điều trị theo hướng cá thể hóa và theo dõi bền vững sau nội soi.",
    description: "Tập trung nhóm bệnh dạ dày, gan mật, đại tràng, tầm soát polyp và ung thư sớm.",
  },
  {
    title: "Phụ sản",
    lead: "Không gian riêng tư, quy trình kín và hỗ trợ liên tục trong suốt thai kỳ.",
    description: "Khám thai, sàng lọc trước sinh, theo dõi nội tiết và chăm sóc phụ khoa định kỳ.",
  },
  {
    title: "Nhi khoa",
    lead: "Mô hình thân thiện cho trẻ nhỏ với tư vấn chủ động cho phụ huynh.",
    description: "Khám bệnh hô hấp, tiêu hóa, dinh dưỡng, miễn dịch và nhắc lịch tiêm chủng, tái khám.",
  },
  {
    title: "Thần kinh - đột quỵ",
    lead: "Tăng tốc sàng lọc nguy cơ và quản lý phục hồi sau điều trị.",
    description: "Điện cơ, MRI, đánh giá đau đầu, rối loạn tiền đình, sa sút trí tuệ và theo dõi đột quỵ.",
  },
  {
    title: "Cơ xương khớp",
    lead: "Kết hợp khám chuyên gia, hình ảnh và tư vấn vận động phục hồi.",
    description: "Theo dõi viêm khớp, thoái hóa, gout, loãng xương và đau cột sống mạn tính.",
  },
];

export const doctors: DoctorProfile[] = [
  {
    name: "PGS.TS.BS Nguyễn Quốc Minh",
    specialty: "Tim mạch",
    title: "Giám đốc Trung tâm Tim mạch can thiệp",
    experience: "20 năm điều trị nội - ngoại tim mạch",
    focus: ["Tăng huyết áp", "Suy tim", "Đánh giá nguy cơ mạch vành"],
  },
  {
    name: "TS.BS Lê Thu Hà",
    specialty: "Phụ sản",
    title: "Trưởng đơn vị Sản phụ khoa cao cấp",
    experience: "17 năm theo dõi thai kỳ nguy cơ và nội tiết sinh sản",
    focus: ["Thai kỳ nguy cơ", "Sàng lọc trước sinh", "Nội tiết phụ khoa"],
  },
  {
    name: "ThS.BS Trần Hữu Nam",
    specialty: "Tiêu hóa",
    title: "Chuyên gia Nội soi và Tiêu hóa lâm sàng",
    experience: "15 năm điều trị bệnh lý gan mật và đại trực tràng",
    focus: ["Nội soi chẩn đoán", "Gan nhiễm mỡ", "Tầm soát ung thư sớm"],
  },
  {
    name: "BSCKII Phạm Khánh Linh",
    specialty: "Nhi khoa",
    title: "Bác sĩ điều phối chăm sóc trẻ em",
    experience: "14 năm điều trị nhi hô hấp và dinh dưỡng",
    focus: ["Viêm hô hấp", "Biếng ăn", "Theo dõi tăng trưởng"],
  },
];

export const patientJourney = [
  "Tiếp nhận đa kênh qua website, tổng đài, Zalo và quầy ưu tiên.",
  "Điều phối lịch tự động theo chuyên khoa, bác sĩ và loại dịch vụ.",
  "Khám lâm sàng, chỉ định cận lâm sàng và cập nhật EMR thời gian thực.",
  "Trả kết quả, kê đơn, thanh toán và nhắc lịch tái khám trong cùng một luồng.",
];

export const newsArticles: NewsArticle[] = [
  {
    category: "Dinh dưỡng lâm sàng",
    title: "Chế độ ăn cho người tăng huyết áp trong 30 ngày đầu điều trị",
    summary: "Những nguyên tắc chọn thực phẩm, phân chia bữa và cách theo dõi đáp ứng tại nhà.",
    readTime: "6 phút đọc",
  },
  {
    category: "Tầm soát sớm",
    title: "Khi nào nên nội soi tiêu hóa gây mê và lịch kiểm tra định kỳ phù hợp",
    summary: "Gợi ý theo nhóm tuổi, tiền sử gia đình và các dấu hiệu cảnh báo không nên bỏ qua.",
    readTime: "8 phút đọc",
  },
  {
    category: "Sản phụ khoa",
    title: "3 mốc xét nghiệm thai kỳ quan trọng cần lên lịch từ sớm",
    summary: "Cách phối hợp siêu âm, double test, NIPT và các mốc tái khám giúp theo dõi toàn diện.",
    readTime: "5 phút đọc",
  },
];

export const footerLinks = {
  services: ["Khám đa khoa", "Xét nghiệm tại nhà", "Khám doanh nghiệp", "Phòng khám chuyên sâu"],
  support: ["Đặt lịch", "Tra cứu kết quả", "Bảng giá", "Hướng dẫn khám bệnh"],
  company: ["Giới thiệu", "Đội ngũ chuyên gia", "Tin tức", "Liên hệ"],
};
