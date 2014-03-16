//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

#import "KSISender.h"

@interface KSFixedSizeMessageSender : NSObject <KSISender>

- (id)initWithPayloadSize:(int)payloadSize;

@end
