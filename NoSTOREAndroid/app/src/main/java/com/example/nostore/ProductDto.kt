import com.google.gson.annotations.SerializedName

data class ProductDto(
    @SerializedName("_id") val _id: String,
    @SerializedName("Name") val Name: String,
    @SerializedName("SEOName") val SEOName: String?,
    @SerializedName("Tags") val Tags: String,
    @SerializedName("Price") val Price: Int,
    @SerializedName("FinalPrice") val FinalPrice: Int,
    @SerializedName("Discount") val Discount: Int,
    @SerializedName("Image") val Image: String,
    @SerializedName("Images") val Images: List<String>?,
    @SerializedName("Quantity") val Quantity: Int,
    @SerializedName("Rating") val Rating: Double,
    @SerializedName("Reviews") val Reviews: List<ReviewDto>?,
    @SerializedName("FinalPriceString") val FinalPriceString: String,
    @SerializedName("Description") val Description: String,
    @SerializedName("Properties") val Properties: Map<String, List<Map<String, String>>>,
    var isFavorite: Boolean = false,
    var inCart: Boolean = false,
    var inCompare: Boolean = false
)

data class ReviewDto(
    @SerializedName("_id") val id: String,
    @SerializedName("ProductId") val productId: String,
    @SerializedName("UserId") val userId: String,
    @SerializedName("User") val user: UserRev,
    @SerializedName("CreateDate") val createDate: String,
    @SerializedName("Rating") val rating: Int,
    @SerializedName("UsageTime") val usageTime: String = "",
    @SerializedName("Pluses") val pluses: String,
    @SerializedName("Minuses") val minuses: String,
    @SerializedName("Comment") val comment: String,
    @SerializedName("Additions") val additions: List<Addition>,
    @SerializedName("Likes") val likes: Int = 0,
    @SerializedName("UsersLikes") val usersLikes: List<String>
)

data class IdWrapper(
    @SerializedName("\$oid") val oid: String
)

data class UserRev(
    @SerializedName("_id") val id: String,
    @SerializedName("Nickname") val nickname: String,
    @SerializedName("AvatarExt") val avatarExt: String?,
)

data class Addition(
    @SerializedName("Date") val date: String,
    @SerializedName("Text") val text: String
)